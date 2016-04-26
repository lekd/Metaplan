using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AppLimit.CloudComputing.SharpBox;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class AnotoNotesDownloader
    {
        #region Public Constructors

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
                InitFolderIfNecessary();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }

            existingNotes = new Dictionary<int, ICloudFileSystemEntry>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Close()
        {
            try
            {
                storage.Close();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public void UpdateNotes()
        {
            List<ICloudFileSystemEntry> newlyUpdatedNotes = getUpdatedNotes(dataFolder, ".txt");
            DownloadUpdatedNote(newlyUpdatedNotes);
        }

        #endregion Public Methods

        #region Private Methods

        private void DownloadUpdatedNote(List<ICloudFileSystemEntry> updatedFileEntries)
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
                        Bitmap bmp = text2Str.FromString(pointsStr,
                            Properties.Settings.Default.AnotoNoteInitWidth, Properties.Settings.Default.AnotoNoteInitHeight,
                            Properties.Settings.Default.AnotoNoteInitLeft, Properties.Settings.Default.AnotoNoteInitTop);
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
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }
        }

        private int getIDfromFileName(string fileName)
        {
            string[] nameComponents = fileName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (nameComponents.Length != 2)
            {
                return -1;
            }
            try
            {
                return Int64.Parse(nameComponents[0]).GetHashCode();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                return -1;
            }
        }

        private List<ICloudFileSystemEntry> getUpdatedNotes(string folderPath, string extensionFilter = ".txt")
        {
            List<ICloudFileSystemEntry> updatedNotes = new List<ICloudFileSystemEntry>();
            ICloudDirectoryEntry curFolder = null;
            try
            {
                curFolder = storage.GetFolder(folderPath);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                return updatedNotes;
            }
            List<ICloudDirectoryEntry> childrenFolders = new List<ICloudDirectoryEntry>();
            List<ICloudFileSystemEntry> childrenFiles = new List<ICloudFileSystemEntry>();
            try
            {
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
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                return updatedNotes;
            }

            //now process the files
            foreach (var file in childrenFiles)
            {
                //only process txt files
                if (!file.Name.Contains(extensionFilter))
                {
                    //writeToFileToDebug("AnotoDebug.txt", file.Name + " not contains .txt");
                    continue;
                }
                //if this is a note generated by Livescribe (file does have ID as filename)
                int ID = getIDfromFileName(file.Name);
                if (ID < 0)
                {
                    continue;
                }
                //if this file is not existing, then just put it in
                if (!existingNotes.ContainsKey(ID))
                {
                    existingNotes.Add(ID, file);
                    updatedNotes.Add(file);
                }
                //otherwise we need to check the modification time to see if it's up-to-date or not
                else
                {
                    if (file.Modified.CompareTo(existingNotes[ID].Modified) > 0)
                    {
                        updatedNotes.Add(file);
                        existingNotes[ID] = file;
                    }
                }
            }
            //continue to process with subfolders
            foreach (var subfolder in childrenFolders)
            {
                string subFolderPath = folderPath + "/" + subfolder.Name;
                List<ICloudFileSystemEntry> subUpdatedFiles = getUpdatedNotes(subFolderPath, ".txt");
                updatedNotes.AddRange(subUpdatedFiles);
            }
            return updatedNotes;
        }

        private void InitFolderIfNecessary()
        {
            try
            {
                storage.GetFolder(dataFolder);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                storage.CreateFolder(dataFolder);
            }
        }

        #endregion Private Methods

        #region Public Events

        public event NewNoteStreamsDownloaded noteStreamsDownloadedHandler = null;

        #endregion Public Events

        #region Private Fields

        private const string dataFolder = "/FromLivescribe";
        private Dictionary<int, ICloudFileSystemEntry> existingNotes = null;
        private CloudStorage storage;
        private ICloudStorageAccessToken storageToken;

        #endregion Private Fields
    }
}