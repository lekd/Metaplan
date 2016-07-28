using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using PostIt_Prototype_1.Utilities;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    using File = Google.Apis.Drive.v3.Data.File;

    public class GoogleDriveFS
    {
        #region Public Constructors

        public GoogleDriveFS(DriveService service)
        {
            UserCredential credential;

            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credPath = System.Environment.GetFolderPath(
                    System.Environment.SpecialFolder.Personal);
                credPath = Path.Combine(credPath, ".credentials/drive-dotnet-quickstart.json");

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
                Console.WriteLine("Credential file saved to: " + credPath);
            }

            // Create Drive API service.
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<File> CreateFolderAsync(string folderPath)
        {
            var fileMetadata = new File();
            fileMetadata.Name = folderPath;
            fileMetadata.MimeType = GoogleMimeTypes.FolderMimeType;
            var request = _service.Files.Create(fileMetadata);
            request.Fields = "id";
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

        public async Task DownloadFileAsync(string fileName, File folder, Stream stream)
        {
            var file = (await DelayedActionAsync(() => GetFileInFolderAsync(fileName, folder)));
            var request = _service.Files.Get(file.Id);
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

        public async Task<File> GetFileFromIdAsync(string Id)
        {
            var request = _service.Files.Get(Id);
            try
            {
                return (await DelayedActionAsync(() => request.ExecuteAsync()));
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public async Task<File> GetFileInFolderAsync(string fileName, File folder)
        {
            var listRequest = _service.Files.List();
            listRequest.Q = $"name='{fileName}' and '{folder.Id}' in parents";
            listRequest.Fields = "files(id, name)";
            try
            {
                var r = await DelayedActionAsync(() => listRequest.ExecuteAsync());
                return r.Files.FirstOrDefault();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public async Task<IList<File>> GetFilesInFolderAsync(File folder)
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.Q = $"'{folder.Id}' in parents";
            listRequest.Fields = "files(id, name, parents)";
            try
            {
                return (await DelayedActionAsync(() => listRequest.ExecuteAsync())).Files;
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        public async Task<File> GetFolderAsync(string folderName)
        {
            FilesResource.ListRequest listRequest = _service.Files.List();
            listRequest.Q = $"name='{folderName}' and mimeType='{GoogleMimeTypes.FolderMimeType}'";
            listRequest.Fields = "files(id, name)";
            try
            {
                var r = await DelayedActionAsync(() => listRequest.ExecuteAsync());
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
            var file = new File()
            {
                Name = fileName,
                MimeType = GoogleMimeTypes.FileMimeType,
                Parents = new List<string>(new[] { folder.Id })
            };

            var request =
                _service.Files.Create(file,
                    stream,
                    "image/png"
                    );

            request.ProgressChanged += RequestOnProgressChanged;
            request.ResponseReceived += RequestOnResponseReceived;

            request.Fields = "id";
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

        private static async Task<T> DelayedActionAsync<T>(Func<Task<T>> action)
        {
            //if (QuotaStopwatch.ElapsedMilliseconds < MinMillisecondsBetweenRequests)
            //await Task.Delay()
            await Task.Delay(MinMillisecondsBetweenRequests);
            var r = await action();
            return r;
        }

        private static async Task DelayedActionAsync(Func<Task> action)
        {
            await DelayedActionAsync(async () =>
            {
                await action();
            });
        }

        private void RequestOnProgressChanged(IUploadProgress uploadProgress)
        {
            if (uploadProgress.Exception != null)
                Debug.WriteLine(uploadProgress.Exception.ToString());
        }

        private void RequestOnResponseReceived(File file)
        {
            Debug.WriteLine("response received!");
        }

        #endregion Private Methods

        #region Public Fields

        public static readonly string ApplicationName = "Metaplan";
        public static readonly string[] Scopes = { DriveService.Scope.Drive };

        #endregion Public Fields

        #region Private Fields

        private const int MinMillisecondsBetweenRequests = (1000 / RequestPerSecondLimit);
        private const int RequestPerSecondLimit = 5;
        private static readonly Stopwatch QuotaStopwatch = new Stopwatch();
        private static DriveService _service;

        #endregion Private Fields
    }

    //static class ExtensionMethods
    //{
    // }
}