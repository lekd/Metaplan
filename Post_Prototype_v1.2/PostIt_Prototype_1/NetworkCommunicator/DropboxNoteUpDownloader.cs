using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using System.IO;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class DropboxNoteUpDownloader
    {
        public delegate void NewNoteStreamsDownloaded(Dictionary<int, Stream> downloadedNoteStream);
        CloudStorage storage;
        ICloudStorageAccessToken storageToken;
        bool isInitialized = false;
        DateTime lastUpdateTime;
        public event NewNoteStreamsDownloaded noteStreamsDownloadedHandler = null;
        public DropboxNoteUpDownloader()
        {
            storage = new CloudStorage();
            var dropboxConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
            ICloudStorageAccessToken accessToken;
            using(var fs = File.Open("DropBoxStorage.Token",FileMode.Open, FileAccess.Read, FileShare.None))
            {
                accessToken = storage.DeserializeSecurityToken(fs);
            }
            storageToken = storage.Open(dropboxConfig, accessToken);
            isInitialized = false;
        }
        public void Test()
        {
            var srcFile = Environment.ExpandEnvironmentVariables("Snapshot.png");
            storage.UploadFile(srcFile, "/Notes/");
        }
        public void UpdateNotes()
        {
            List<ICloudFileSystemEntry> updatedFileEntries = getUpdatedNotes("/Notes");
            if (updatedFileEntries.Count > 0)
            {
                if (!isInitialized)
                {
                    isInitialized = true;
                }
                lastUpdateTime = findTheLatestUpdateTime(updatedFileEntries);
                DownloadUpdatedNotes(updatedFileEntries);
            }
        }
        //download all recently-updated notes and return them together with their corresponding IDs
        void DownloadUpdatedNotes(List<ICloudFileSystemEntry> updatedFileEntries)
        {


            foreach (ICloudFileSystemEntry fileEntry in updatedFileEntries)
            {
                Dictionary<int, Stream> noteFiles = new Dictionary<int, Stream>();
                var containingFolder = fileEntry.Parent;
                using (MemoryStream memStream = new MemoryStream())
                {
                    storage.DownloadFile(fileEntry.Name, containingFolder, memStream);
                    memStream.Seek(0, 0);
                    //extract ID
                    String[] nameComponents = fileEntry.Name.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                    int ID = -1;
                    if (nameComponents.Length > 0)
                    {
                        Int32.TryParse(nameComponents[0], out ID);
                    }
                    noteFiles.Add(ID, memStream);
                }
                if (noteStreamsDownloadedHandler != null)
                {
                    noteStreamsDownloadedHandler(noteFiles);
                }
            }
        }
        public void UpdateMetaplanBoardScreen(MemoryStream screenshotStream)
        {
            ICloudDirectoryEntry targetFolder = storage.GetFolder("/");
            storage.UploadFile(screenshotStream, "MetaplanBoard.png", targetFolder);
        }
        public void UpdateMetaplanBoardScreen(byte[] screenshotBytes)
        {

            using (MemoryStream stream = new MemoryStream(screenshotBytes))
            {
                ICloudDirectoryEntry targetFolder = storage.GetFolder("/");
                storage.UploadFile(stream, "MetaplanBoard.png", targetFolder);
            }
        }
        public void UpdateMetaplanBoardScreen(string screenshotFileName)
        {
            
            ICloudDirectoryEntry targetFolder = storage.GetFolder("/");
            storage.UploadFile(screenshotFileName, targetFolder);
        }
        DateTime findTheLatestUpdateTime(List<ICloudFileSystemEntry> updatedNotes)
        {
            DateTime latestUpdateTime = updatedNotes[0].Modified;
            foreach (var note in updatedNotes)
            {
                if (note.Modified.CompareTo(latestUpdateTime) > 0)
                {
                    latestUpdateTime = note.Modified;
                }
            }
            return latestUpdateTime;
        }
        public List<ICloudFileSystemEntry> getUpdatedNotes(string folderPath)
        {
            var curFolder = storage.GetFolder(folderPath);
            List<ICloudFileSystemEntry> updatedChildrenFiles = new List<ICloudFileSystemEntry>();
            List<ICloudDirectoryEntry> childrenFolders = new List<ICloudDirectoryEntry>();
            foreach (var fof in curFolder)
            {
                if (fof is ICloudDirectoryEntry)
                {
                    childrenFolders.Add((ICloudDirectoryEntry)fof);
                }
                else
                {
                    //if this is the first check of the Dropbox folder
                    if (!isInitialized)
                    {
                        updatedChildrenFiles.Add(fof);
                    }
                    else
                    {
                        
                        //this file has been just recently updated since the last update
                        if (fof.Modified.CompareTo(lastUpdateTime)>0)
                        {
                            updatedChildrenFiles.Add(fof);
                        }
                    }
                }
            }
            foreach(var subfolder in childrenFolders)
            {
                string subFolderPath = folderPath + "/" + subfolder.Name;
                List<ICloudFileSystemEntry> subUpdatedFiles = getUpdatedNotes(subFolderPath);
                updatedChildrenFiles.AddRange(subUpdatedFiles);
            }
            return updatedChildrenFiles;
        }
        public void Close()
        {
            storage.Close();
        }
    }
}
