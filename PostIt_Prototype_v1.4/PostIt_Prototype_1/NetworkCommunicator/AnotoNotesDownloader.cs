using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Windows;
using System.Globalization;
using System.Drawing;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class AnotoNotesDownloader
    {
        CloudStorage storage;
        ICloudStorageAccessToken storageToken;
        Dictionary<int, ICloudFileSystemEntry> existingAnotoNotes = null;

        public event PostIt_Prototype_1.NetworkCommunicator.DropboxNoteUpDownloader.NewNoteStreamsDownloaded noteStreamsDownloadedHandler = null;
        public AnotoNotesDownloader()
        {
            try
            {
                storage = new CloudStorage();
                var dropboxConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
                ICloudStorageAccessToken accessToken;
                using (var fs = File.Open(Properties.Settings.Default.DropboxTokenFile, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    accessToken = storage.DeserializeSecurityToken(fs);
                }
                storageToken = storage.Open(dropboxConfig, accessToken);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.writeToFileToDebug(Properties.Settings.Default.DebugLogFile, "AnotoNotesDownloader: " + ex.Message);
            }

            existingAnotoNotes = new Dictionary<int, ICloudFileSystemEntry>();
        }
        public void UpdateNotes()
        {

            List<ICloudFileSystemEntry> newlyUpdatedNotes = getUpdatedAnotoNotes("/FromLivescribe");
            DownloadUpdatedNote(newlyUpdatedNotes);
        }
        void DownloadUpdatedNote(List<ICloudFileSystemEntry> updatedFileEntries)
        {
            var text2Str = new Utilities.PointStringToBMP(Properties.Settings.Default.AnotoNoteScale);
            foreach (ICloudFileSystemEntry fileEntry in updatedFileEntries)
            {
                try
                {
                    Dictionary<int, Stream> noteFiles = new Dictionary<int, Stream>();
                    var containingFolder = fileEntry.Parent;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        storage.DownloadFile(fileEntry.Name, containingFolder, memStream);
                        memStream.Seek(0, 0);
                        int noteID = getIDfromFileName(fileEntry.Name);

                        StreamReader reader = new StreamReader(memStream);
                        var pointsStr = reader.ReadToEnd();
                        Bitmap bmp = text2Str.FromString(pointsStr, Properties.Settings.Default.AnotoNoteInitWidth, Properties.Settings.Default.AnotoNoteInitHeight);
                        byte[] bmpBytes = Utilities.UtilitiesLib.BitmapToBytes(bmp);
                        using (MemoryStream bmpMemStream = new MemoryStream(bmpBytes))
                        {
                            noteFiles.Add(noteID, bmpMemStream);
                        }

                    }
                    if (noteStreamsDownloadedHandler != null)
                    {
                        noteStreamsDownloadedHandler(noteFiles);
                    }
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.writeToFileToDebug(Properties.Settings.Default.DebugLogFile, "AnotoNotesDownLoader-DownloadUpdatedNote: " + ex.Message);
                }
                
            }
        }
        List<ICloudFileSystemEntry> getUpdatedAnotoNotes(string folderPath)
        {
            List<ICloudFileSystemEntry> updatedNotes = new List<ICloudFileSystemEntry>();
            ICloudDirectoryEntry curFolder = null;
            try
            {
                curFolder = storage.GetFolder(folderPath);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.writeToFileToDebug(Properties.Settings.Default.DebugLogFile, "DropboxNoteUpDownLoader: " + ex.Message);
                return updatedNotes;
            }
            List<ICloudDirectoryEntry> childrenFolders = new List<ICloudDirectoryEntry>();
            List<ICloudFileSystemEntry> childrenFiles = new List<ICloudFileSystemEntry>();
            foreach (var fof in curFolder)
            {
                if (fof is ICloudDirectoryEntry)
                {
                    childrenFolders.Add((ICloudDirectoryEntry)fof);
                }
                else
                {
                    childrenFiles.Add(fof);
                }
            }
            //now process the files
            foreach (var file in childrenFiles)
            {
                //only process txt files
                if (!file.Name.Contains(".txt"))
                {
                    //writeToFileToDebug("AnotoDebug.txt", file.Name + " not contains .txt");
                    continue;
                }
                //if this is a note generated by Livescribe (file does have ID as filename)
                int ID = getIDfromFileName(file.Name);
                if (ID < 0)
                {
                    //writeToFileToDebug("AnotoDebug.txt", file.Name + " not have ID");
                    continue;
                }
                //if this file is not existing, then just put it in
                if (!existingAnotoNotes.ContainsKey(ID))
                {
                    existingAnotoNotes.Add(ID, file);
                    updatedNotes.Add(file);
                }
                //otherwise we need to check the modification time to see if it's up-to-date or not
                else
                {
                    if (file.Modified.CompareTo(existingAnotoNotes[ID].Modified) > 0)
                    {
                        updatedNotes.Add(file);
                        existingAnotoNotes[ID] = file;
                    }
                }
            }
            //continue to process with subfolders
            foreach (var subfolder in childrenFolders)
            {
                string subFolderPath = folderPath + "/" + subfolder.Name;
                List<ICloudFileSystemEntry> subUpdatedFiles = getUpdatedAnotoNotes(subFolderPath);
                updatedNotes.AddRange(subUpdatedFiles);
            }
            return updatedNotes;
        }
        int getIDfromFileName(string fileName)
        {
            string[] nameComponents = fileName.Split(new string[] { "." },StringSplitOptions.RemoveEmptyEntries);
            if (nameComponents.Length != 2)
            {
                return -1;
            }
            try
            {
                return  Int64.Parse(nameComponents[0]).GetHashCode();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.writeToFileToDebug(Properties.Settings.Default.DebugLogFile, "AnotoNotesDownLoader: " + ex.Message);
                return -1;
            }
        }
        public void Close()
        {
            try
            {
                storage.Close();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.writeToFileToDebug(Properties.Settings.Default.DebugLogFile, "AnotoNotesDownLoader: " + ex.Message);
            }
        }
    }
}
