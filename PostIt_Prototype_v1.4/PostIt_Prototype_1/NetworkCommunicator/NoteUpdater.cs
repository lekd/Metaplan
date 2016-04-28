using System;
using System.Collections.Generic;

//using AppLimit.CloudComputing.SharpBox;
using System.IO;
using AppLimit.CloudComputing.SharpBox;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class NoteUpdater
    {
        #region Public Constructors
        static NoteUpdater()
        {

        }
        public NoteUpdater(string searchPattern = ".png")
        {
            SearchPattern = searchPattern;
            try
            {                
                Storage = new CloudStorage();
                var dropboxConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
                ICloudStorageAccessToken accessToken;
                using (var fs = File.Open(Properties.Settings.Default.DropboxTokenFile, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    accessToken = Storage.DeserializeSecurityToken(fs);
                }
                storageToken = Storage.Open(dropboxConfig, accessToken);
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
                Storage.Close();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public void UpdateNotes()
        {
            List<ICloudFileSystemEntry> newlyUpdatedNotes = getUpdatedNotes(dataFolder, SearchPattern);
            DownloadUpdatedNotes(newlyUpdatedNotes);
        }

        #endregion Public Methods

        #region Private Methods

        //download all recently-updated image notes and return them together with their corresponding IDs
        private void DownloadUpdatedNotes(List<ICloudFileSystemEntry> updatedFileEntries)
        {
            foreach (ICloudFileSystemEntry fileEntry in updatedFileEntries)
            {
                try
                {
                    Dictionary<int, Stream> noteFiles = new Dictionary<int, Stream>();
                    var containingFolder = fileEntry.Parent;
                    using (MemoryStream memStream = new MemoryStream())
                    {
                        Storage.DownloadFile(fileEntry.Name, containingFolder, memStream);
                        memStream.Seek(0, 0);
                        //extract ID
                        int noteID = getIDfromFileName(fileEntry.Name);
                        ProcessMemStream(noteFiles, memStream, noteID);                        
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

        protected virtual void ProcessMemStream(Dictionary<int, Stream> noteFiles, MemoryStream memStream, int noteID)
        {
            noteFiles.Add(noteID, memStream);
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

        private List<ICloudFileSystemEntry> getUpdatedNotes(string folderPath, string extensionFilter = ".png")
        {
            List<ICloudFileSystemEntry> updatedNotes = new List<ICloudFileSystemEntry>();
            ICloudDirectoryEntry curFolder = null;
            try
            {
                curFolder = Storage.GetFolder(folderPath);
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
                List<ICloudFileSystemEntry> subUpdatedFiles = getUpdatedNotes(subFolderPath, extensionFilter);
                updatedNotes.AddRange(subUpdatedFiles);
            }
            return updatedNotes;
        }

        private void InitFolderIfNecessary()
        {
            try
            {
                Storage.GetFolder(dataFolder);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                Storage.CreateFolder(dataFolder);
            }
        }

        #endregion Private Methods

        #region Public Events

        public event NewNoteStreamsDownloaded noteStreamsDownloadedHandler = null;

        #endregion Public Events

        #region Public Properties

        public CloudStorage Storage { get; private set; }
        public string SearchPattern { get; protected set; }

        #endregion Public Properties

        #region Private Fields

        private const string dataFolder = "/Notes";

        private Dictionary<int, ICloudFileSystemEntry> existingNotes = null;

        private ICloudStorageAccessToken storageToken;

        #endregion Private Fields
    }
    public delegate void NewNoteStreamsDownloaded(Dictionary<int, Stream> downloadedNoteStream);

}