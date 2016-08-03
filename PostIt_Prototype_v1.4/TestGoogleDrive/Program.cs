using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using PostIt_Prototype_1.NetworkCommunicator;

namespace TestGoogleDrive
{
    class Program
    {
        private static DriveService _service;

        private static DriveService InitDrive()
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
            return _service;
        }

        public static string[] Scopes = { DriveService.Scope.Drive };

        public static string ApplicationName = "Test Google FS";

        static void Main(string[] args)
        {
            //AsyncContext.Run(() => MainAsync(args));
            Console.WriteLine("Started");
            MainAsync(args).Wait();

            Console.WriteLine(@"Press any key to continue...");
            Console.ReadLine();
        }

        static async Task MainAsync(string[] args)
        {
            var gdfs = new GoogleDriveFS();
            
            var files = await gdfs.GetChildrenAsync(await gdfs.GetFolderAsync("MercoNotes"));
            var sw = new Stopwatch();
            sw.Start();
            int fileNo = 0;
            foreach (var f in files)
            {

                Console.WriteLine($@"{f.Name}-{f.Id}-{f.MimeType}");
                var s = new MemoryStream();

                if (f.MimeType == "image/png")
                {
                    fileNo++;
                    await gdfs.DownloadFileAsync(f, s);
                }
            }
            sw.Stop();
            Console.WriteLine($@"Downloading {fileNo} files took {sw.Elapsed} seconds.");

        }
    }
}
