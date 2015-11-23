﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Windows;
using System.Diagnostics;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class DropboxNoteUpDownloader
    {
        public delegate void NewNoteStreamsDownloaded(Dictionary<int, Stream> downloadedNoteStream);
        CloudStorage storage;
        ICloudStorageAccessToken storageToken;
        Dictionary<int, ICloudFileSystemEntry> existingNotes = null;

        public event NewNoteStreamsDownloaded noteStreamsDownloadedHandler = null;
        public DropboxNoteUpDownloader()
        {
            storage = new CloudStorage();
            var dropboxConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.DropBox);
            ICloudStorageAccessToken accessToken;
            using (var fs = File.Open(Properties.Settings.Default.DropboxTokenFile, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                accessToken = storage.DeserializeSecurityToken(fs);
            }
            storageToken = storage.Open(dropboxConfig, accessToken);
            existingNotes = new Dictionary<int, ICloudFileSystemEntry>();

            InitNoteFolderIfNecessary();
        }
        void InitNoteFolderIfNecessary()
        {
            try
            {
                storage.GetFolder("/Notes");
            }
            catch (Exception ex)
            {
                storage.CreateFolder("/Notes");
            }
            
        }
        public void UpdateNotes()
        {
            List<ICloudFileSystemEntry> updatedGeneralNoteFileEntries = getUpdatedNotes("/Notes");
            DownloadUpdatedNotes(updatedGeneralNoteFileEntries);
        }

        //download all recently-updated image notes and return them together with their corresponding IDs
        void DownloadUpdatedNotes(List<ICloudFileSystemEntry> updatedFileEntries)
        {
            foreach (ICloudFileSystemEntry fileEntry in updatedFileEntries)
            {
                Dictionary<int, Stream> noteFiles = new Dictionary<int, Stream>();
                var containingFolder = fileEntry.Parent;
                using (MemoryStream memStream = new MemoryStream())
                {
                    try
                    {
                        storage.DownloadFile(fileEntry.Name, containingFolder, memStream);
                        memStream.Seek(0, 0);
                        //extract ID
                        String[] nameComponents = fileEntry.Name.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
                        int ID = getIDfromFileName(fileEntry.Name);
                        noteFiles.Add(ID, memStream);
                    }
                    catch (Exception ex)
                    {
                        Utilities.UtilitiesLib.writeToFileToDebug("errorlog.txt", "DropboxNoteUpDownLoader: " + ex.Message);
                    }
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
            try
            {
                storage.UploadFile(screenshotStream, "MetaplanBoard.png", targetFolder);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
        public List<ICloudFileSystemEntry> getUpdatedNotes(string folderPath)
        {
            List<ICloudFileSystemEntry> updatedNotes = new List<ICloudFileSystemEntry>();
            ICloudDirectoryEntry curFolder = null;
            try
            {
                curFolder = storage.GetFolder(folderPath);
            }
            catch(Exception ex)
            {
                Utilities.UtilitiesLib.writeToFileToDebug("errorlog.txt", "DropboxNoteUpDownLoader: " + ex.Message);
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
                if (!file.Name.Contains(".png"))
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
            foreach(var subfolder in childrenFolders)
            {
                string subFolderPath = folderPath + "/" + subfolder.Name;
                List<ICloudFileSystemEntry> subUpdatedFiles = getUpdatedNotes(subFolderPath);
                updatedNotes.AddRange(subUpdatedFiles);
            }
            return updatedNotes;
        }
        int getIDfromFileName(string fileName)
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
                Utilities.UtilitiesLib.writeToFileToDebug("errorlog.txt", "DropboxNoteUpDownLoader: " + ex.Message);
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
                Utilities.UtilitiesLib.writeToFileToDebug("errorlog.txt", "DropboxNoteUpDownLoader: " + ex.Message);
            }
        }
    }
}
