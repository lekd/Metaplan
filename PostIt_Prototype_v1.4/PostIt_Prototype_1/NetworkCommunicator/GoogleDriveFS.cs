using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Google;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using WhiteboardApp.Utilities;

namespace WhiteboardApp.NetworkCommunicator
{
    using File = Google.Apis.Drive.v3.Data.File;

    // ReSharper disable once InconsistentNaming
    public class GoogleDriveFS : ICloudFS<File>
    {
        // TODO: Take care of 100 item per page limit

        #region Public Constructors

        static GoogleDriveFS()
        {
            if (Service != null)
                return;

            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/metaplan.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Debug.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            Service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        #endregion Public Constructors

        #region Public Methods

        public File CreateFolder(string folderName)
        {
            return CreateFolder(folderName, AppDataFolder);
        }

        public File CreateFolder(string folderName, File parent)
        {
            Debug.WriteLine($"CreateFolder {folderName} {parent}");
            var request = CreateFolderRequest(folderName, parent);

            try
            {
                var r = DelayedAction(() => request.Execute());
                return r;
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public Task<File> CreateFolderAsync(string folderName)
        {
            return CreateFolderAsync(folderName, AppDataFolder);
        }

        public async Task<File> CreateFolderAsync(string folderName, File parent)
        {
            Debug.WriteLine($"CreateFolderAsync {folderName} {parent.Name}.");

            var request = CreateFolderRequest(folderName, parent);

            try
            {
                var r = await DelayedActionAsync(() => request.ExecuteAsync());
                return r;
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public async Task DownloadFileAsync(File file, Stream stream)
        {
            Debug.WriteLine($"DownloadFileAsync {file.Name}.");

            var request = Service.Files.Get(file.Id);

            try
            {
                await DelayedActionAsync(() => request.DownloadAsync(stream));
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public async Task<IList<File>> GetChildrenAsync(File folder, GoogleMimeType mimeType = null)
        {
            Debug.WriteLine($"GetChildrenAsync {folder.Name} {mimeType}.");

            try
            {
                return (await DelayedActionAsync(() => GetListRequest(folder, mimeType).ExecuteAsync())).Files;
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        /// <summary>
        /// Gets a folder from appDataFolder
        /// </summary>
        /// <param name="folderName">Folder's name to look for</param>
        /// <returns>Folder's meta data, or null if folder not found.</returns>
        public File GetFolder(string folderName)
        {
            return GetFolder(folderName, AppDataFolder);
        }

        /// <summary>
        /// Gets folder's meta data
        /// </summary>
        /// <param name="folderName">Folder's name to look for</param>
        /// <param name="parent">Folder's parent</param>
        /// <returns>Folder's meta data, or null if folder not found.</returns>
        public File GetFolder(string folderName, File parent)
        {
            Debug.WriteLine($"Getting folder {folderName} in {parent.Id}");
            try
            {
                var r = DelayedAction(() => GetListRequest(folderName, parent, GoogleMimeTypes.FolderMimeType).Execute());
                return r.Files.FirstOrDefault();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public Task<File> GetFolderAsync(string folderName)
        {
            return GetFolderAsync(folderName, AppDataFolder);
        }

        public async Task<File> GetFolderAsync(string folderName, File parent)
        {
            Debug.WriteLine($"GetFolderAsync {folderName} {parent.Id}");
            try
            {
                var r = await DelayedActionAsync(() => GetListRequest(folderName, parent, GoogleMimeTypes.FolderMimeType).ExecuteAsync());
                return r.Files.FirstOrDefault();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public async Task<File> UploadFileAsync(Stream stream, string fileName, File folder)
        {
            Debug.WriteLine($"UploadFileAsync {fileName} {folder}");

            var file = new File()
            {
                Name = fileName,
                MimeType = GoogleMimeTypes.FileMimeType,
                Parents = new List<string>(new[] { folder.Id }),
                Spaces = new[] { AppDataFolderSpace }
            };

            var request =
                Service.Files.Create(file,
                    stream,
                    "image/png"
                    );

            request.ProgressChanged += RequestOnProgressChanged;
            request.ResponseReceived += RequestOnResponseReceived;

            request.Fields = "id, name";
            try
            {
                await DelayedActionAsync(() => request.UploadAsync());
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
            return request.ResponseBody;
        }

        public async Task<File> UploadFileAsync(string localFilePath, File targetFolder)
        {
            Debug.WriteLine($"UploadFileAsync {localFilePath} {targetFolder}");

            using (var stream = new System.IO.FileStream(localFilePath, System.IO.FileMode.Open))
            {
                try
                {
                    // ReSharper disable AccessToDisposedClosure
                    return (await DelayedActionAsync(() => UploadFileAsync(stream, Path.GetFileName(localFilePath), targetFolder)));
                    // ReSharper restore AccessToDisposedClosure
                }
                catch (WebException ex)
                {
                    MessageBox.Show(ex.Response.ToString());
                    throw;
                }
            }
        }

        #endregion Public Methods

        #region Private Methods

        private static string AndQueries(params string[] queries)
        {
            var filtered = from q in queries where !string.IsNullOrEmpty(q) select q;

            return string.Join(" and ", filtered);
        }

        private static FilesResource.CreateRequest CreateFolderRequest(string folderPath, File parent)
        {
            var fileMetadata = new File
            {
                Name = folderPath,
                MimeType = GoogleMimeTypes.FolderMimeType
            };
            if (parent != null)
                fileMetadata.Parents = new List<string> { parent.Id };

            //fileMetadata.Spaces = new[] { AppDataFolderSpace };

            var request = Service.Files.Create(fileMetadata);
            request.Fields = "id";
            return request;
        }

        private static T DelayedAction<T>(Func<T> action)
        {
            T r;
            try
            {
                r = action();
            }
            catch (GoogleApiException)
            {
                Task.Delay(MinMillisecondsBetweenRequests);
                r = action();
            }

            return r;
        }

        private static async Task<T> DelayedActionAsync<T>(Func<Task<T>> action)
        {
            T r;
            try
            {
                r = await action();
            }
            catch (GoogleApiException)
            {
                await Task.Delay(MinMillisecondsBetweenRequests);
                r = await action();
            }

            return r;
        }

        private static async Task DelayedActionAsync(Func<Task> action)
        {
            await DelayedActionAsync(async () =>
            {
                await action();
            });
        }

        private static FilesResource.ListRequest GetListRequest(string folderName, GoogleMimeType mimeType)
        {
            return GetListRequest(folderName, null, mimeType);
        }

        private static FilesResource.ListRequest GetListRequest(File parent, GoogleMimeType mimeType)
        {
            return GetListRequest(null, parent, mimeType);
        }

        private static FilesResource.ListRequest GetListRequest(string folderName, File parent, GoogleMimeType mimeType = null)
        {
            var listRequest = Service.Files.List();
            listRequest.Q = AndQueries(
                folderName != null ? $"name='{folderName}'" : "",
                mimeType != null ? $"mimeType='{mimeType}'" : "",
                parent != null ? $"'{parent.Id}' in parents" : "",
                "trashed!=true"
                );

            listRequest.Spaces = AppDataFolderSpace;

            listRequest.Fields = "files(id, name, parents, mimeType)";
            return listRequest;
        }

        private void RequestOnProgressChanged(IUploadProgress uploadProgress)
        {
            if (uploadProgress.Exception != null)
                Debug.WriteLine(uploadProgress.Exception.ToString());
        }

        private void RequestOnResponseReceived(File file)
        {
            Debug.WriteLine($"Uploaded {file.Id}:{file.Name}:{file.Size}");
        }

        #endregion Private Methods

        #region Public Properties

        public static string ApplicationName => "Metaplan";

        #endregion Public Properties

        #region Public Fields

        public static readonly string[] Scopes = { DriveService.Scope.DriveAppdata };
        public static File AppDataFolder = new File() { Id = AppDataFolderSpace };

        #endregion Public Fields

        #region Private Fields

        private const string AppDataFolderSpace = "appDataFolder";
        private const int MinMillisecondsBetweenRequests = (1000 / RequestPerSecondLimit);
        private const int RequestPerSecondLimit = 5;
        private static readonly DriveService Service;

        #endregion Private Fields
    }

    //static class ExtensionMethods
    //{
    // }
}