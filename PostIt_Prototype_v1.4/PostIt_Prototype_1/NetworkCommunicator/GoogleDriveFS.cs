using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AppLimit.CloudComputing.SharpBox;
using Google.Apis.Drive.v3;
using PostIt_Prototype_1.Utilities;
using File = Google.Apis.Drive.v3.Data.File;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    using Folder = File;

    class GoogleDriveFS : IGenericCloudFS<File, Folder>
    {
        private static DriveService _service;

        public Folder GetFolder(string folderName)
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.Q = $"name='{folderName}' and mimeType='{GoogleMimeTypes.FolderMimeType}'";
            listRequest.Fields = "files(id, name)";
            return listRequest.Execute().Files.FirstOrDefault();
        }

        private static IList<File> GetFilesInFolder(Folder folder)
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.Q = $"'{folder.Id}' in parents";
            listRequest.Fields = "files(id, name)";
            return listRequest.Execute().Files;
        }

        private static File GetFileInFolder(string fileName, Folder folder)
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.Q = $"name='{fileName}' and '{folder.Id}' in parents";
            listRequest.Fields = "files(id, name)";
            return listRequest.Execute().Files.FirstOrDefault();
        }

        public Folder CreateFolder(string folderPath)
        {
            var fileMetadata = new File();
            fileMetadata.Name = folderPath;
            fileMetadata.MimeType = GoogleMimeTypes.FolderMimeType;
            var request = _service.Files.Create(fileMetadata);
            request.Fields = "id";
            return request.Execute();
        }

        public void DownloadFile(string fileName, Folder folder, Stream stream)
        {
            var file = GetFileInFolder(fileName, folder);
            var request = _service.Files.Get(file.Id);
            request.Download(stream);
        }

        public File UploadFile(Stream stream, string fileName, Folder folder)
        {

            var request = 
                _service.Files.Create(
                    new File() { Name = fileName, MimeType = GoogleMimeTypes.FileMimeType },
                    stream,
                    "image/png"
                    );
            request.Fields = "id";
            request.Upload();
            return request.ResponseBody;
        }

        public File UploadFile(string localFilePath, Folder targetFolder)
        {
            using (var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open))
            {
                return UploadFile(stream, Path.GetFileName(localFilePath), targetFolder);
            }
        }
    }
}
