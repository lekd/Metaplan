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
using PostIt_Prototype_1.Utilities;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    using Google.Apis.Drive.v3.Data;
    using File = Google.Apis.Drive.v3.Data.File;

    // ReSharper disable once InconsistentNaming
    public class GoogleDriveFS
    {
        // TODO: Take care of 100 item per page limit 
        #region Public Constructors

        static GoogleDriveFS()
        {
            if (_service != null)
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
            _service = new DriveService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<File> CreateFolderAsync(string folderPath, File parent = null)
        {
            var fileMetadata = new File
            {
                Name = folderPath,
                MimeType = GoogleMimeTypes.FolderMimeType
            };
            if (parent != null)
                fileMetadata.Parents = new List<string> { parent.Id };

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

        public async Task DownloadFileAsync(File file, Stream stream)
        {
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

        public async Task DownloadFileAsync(string fileName, File folder, Stream stream)
        {
            var file = (await DelayedActionAsync(() => GetFileInFolderAsync(fileName, folder)));
            await DownloadFileAsync(file, stream);
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
            listRequest.Q = AndQueries(
                $"name='{fileName}'",
                $"'{folder.Id}' in parents"
                );

            listRequest.Fields = "files(id, name, mimeType)";
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

        public async Task<IList<File>> GetChildrenAsync(File folder, GoogleMimeType mimeType = null)
        {
            var listRequest = _service.Files.List();
            listRequest.Q = AndQueries(
                $"'{folder.Id}' in parents",
                "trashed != true",
                mimeType != null ? $"mimeType = '{mimeType}'" : ""
                );

            listRequest.Fields = "files(id, name, parents, mimeType)";
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

        public async Task<File> GetFolderAsync(string folderName, File parent = null)
        {
            var listRequest = GetList(folderName, parent);
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

        public File GetFolder(string folderName, File parent = null)
        {
            var listRequest = GetList(folderName, parent);
            try
            {
                var r = DelayedAction(() => listRequest.Execute());
                return r.Files.FirstOrDefault();
            }
            catch (WebException ex)
            {
                MessageBox.Show(ex.Response.ToString());
                throw;
            }
        }

        private static FilesResource.ListRequest GetList(string folderName, File parent)
        {
            var listRequest = _service.Files.List();

            listRequest.Q = AndQueries(
                $"name='{folderName}'",
                $"mimeType='{GoogleMimeTypes.FolderMimeType}'",
                "trashed!=true",
                parent != null ? $"'{parent.Id}' in parents" : ""
                );
            listRequest.Fields = "files(id, name, parents)";
            return listRequest;
        }

        private static string AndQueries(params string[] queries)
        {
            var filtered = from q in queries where !string.IsNullOrEmpty(q) select q;

            return string.Join(" and ", filtered);
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
        #region Private Methods

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

        #region Public Fields

        public static readonly string ApplicationName = "Metaplan";
        public static readonly string[] Scopes = { DriveService.Scope.DriveAppdata };

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