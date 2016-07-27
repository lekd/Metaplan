using System;
using System.IO;
using AppLimit.CloudComputing.SharpBox;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class DropboxFS: ICloudFS<ICloudFileSystemEntry, ICloudDirectoryEntry>
    {
        private CloudStorage _storage;

        public ICloudDirectoryEntry GetFolder(string folderPath)
        {
            return _storage.GetFolder(folderPath);
        }

        public ICloudDirectoryEntry CreateFolder(string folderPath)
        {
            return _storage.CreateFolder(folderPath);
        }

        public void DownloadFile(string fileName, ICloudDirectoryEntry folder, Stream stream)
        {
             _storage.DownloadFile(fileName, folder, stream);
        }

        public ICloudFileSystemEntry UploadFile(Stream stream, string fileName, ICloudDirectoryEntry folder)
        {
            return _storage.UploadFile(stream, fileName, folder);
        }

        public ICloudFileSystemEntry UploadFile(string localFilePath, ICloudDirectoryEntry targetFolder)
        {
            return _storage.UploadFile(localFilePath, targetFolder);
        }

        public DropboxFS(CloudStorage storage) 
        {
            _storage = storage;
        }

    }
}