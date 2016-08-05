using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;
using Dropbox.Api.Files;
using PostIt_Prototype_1.Utilities;
namespace PostIt_Prototype_1.NetworkCommunicator
{

    public class DropboxFS : ICloudFS<Metadata>
    {
        private static DropboxClient dbClient;
        private const string ACCESS_TOKEN = "s7WRjU1-5tAAAAAAAAAA8c2K-AZCSrIGg2vC-eWwthEKEwY2S4fIQnpIYz4LyQNI";
        static DropboxFS()
        {
            dbClient = new DropboxClient(ACCESS_TOKEN);
            Debug.WriteLine(dbClient.Users.GetCurrentAccountAsync().Result.Name.DisplayName);
        }

        public Metadata CreateFolder(string folderName)
        {
            return Task.Run(() => CreateFolderAsync(folderName)).Result;
        }

        public Metadata CreateFolder(string folderName, Metadata parent)
        {
            return Task.Run(() => CreateFolderAsync(folderName, parent)).Result;
        }

        public async Task<Metadata> CreateFolderAsync(string folderName)
        {
            if (!folderName.StartsWith("/"))
                folderName = $"/{folderName}";

            return await dbClient.Files.CreateFolderAsync(folderName);
        }

        public async Task<Metadata> CreateFolderAsync(string folderName, Metadata parent)
        {
            return await CreateFolderAsync(Dropbox_Path_Combine(parent.PathLower, folderName));
        }

        public async Task DownloadFileAsync(Metadata file, Stream stream)
        {
            using (var response = await dbClient.Files.DownloadAsync(file.PathLower))
            {
                var r = await response.GetContentAsStreamAsync();
                await r.CopyToAsync(stream);
            }
        }

        public async Task<IList<Metadata>> GetChildrenAsync(Metadata folder, GoogleMimeType mimeType = null)
        {
            var entries = (await dbClient.Files.ListFolderAsync(folder.PathLower)).Entries;

            if (mimeType == null)
                return entries;

            var filtered = from entry in entries
                           where (mimeType.Equals(GoogleMimeTypes.FolderMimeType) ? entry.IsFolder : entry.IsFile)
                           select entry;

            return filtered.ToList();
        }

        public Metadata GetFolder(string folderName)
        {
            return Task.Run(() => GetFolderAsync(folderName)).Result;
        }

        public Metadata GetFolder(string folderName, Metadata parent)
        {
            return Task.Run(() => GetFolderAsync(folderName, parent)).Result;
        }

        public async Task<Metadata> GetFolderAsync(string folderName)
        {

            if (!folderName.StartsWith("/"))
                folderName = $"/{folderName}";
            try
            {
                var p = await dbClient.Files.GetMetadataAsync(folderName);
                return p;
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Metadata> GetFolderAsync(string folderName, Metadata parent)
        {
            return await GetFolderAsync(Dropbox_Path_Combine(parent.PathLower, folderName));
        }

        private string Dropbox_Path_Combine(string pathLower, string folderName)
        {
            if (!pathLower.StartsWith("/"))
                pathLower = $"/{pathLower}";

            if (!folderName.StartsWith("/"))
                folderName = $"/{folderName}";

            return pathLower + folderName;
        }

        public async Task<Metadata> UploadFileAsync(string localFilePath, Metadata targetFolder)
        {
            using (var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open))
            {
                return await UploadFileAsync(stream, Path.GetFileName(localFilePath), targetFolder);
            }
        }

        public async Task<Metadata> UploadFileAsync(Stream stream, string fileName, Metadata folder)
        {
            return await dbClient.Files.UploadAsync(
                Dropbox_Path_Combine(folder.PathLower, fileName),
                WriteMode.Overwrite.Instance,
                body: stream);
        }
    }
}
