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
using PostIt_Prototype_1.Utilities;
using File = Google.Apis.Drive.v3.Data.File;

namespace TestGoogleDrive
{
    class Program
    {
        private static GoogleDriveFS _gdfs;

        static void Main(string[] args)
        {
            Console.WriteLine(@"Started");
            MainAsync(args).Wait();

            Console.WriteLine(@"Press any key to continue...");
            Console.ReadLine();
        }

        public static string Tabs(int n)
        {
            return new string('\t', n);
        }
        static async Task MainAsync(string[] args)
        {
            _gdfs = new GoogleDriveFS();
            var files = await _gdfs.GetChildrenAsync(GoogleDriveFS.AppDataFolder);
            var sw = await Dir(files, recursive: true);
            Console.WriteLine($@"Listing files took {sw.Elapsed} seconds.");
        }

        private static async Task<Stopwatch> Dir(IEnumerable<File> files, int level = 0, bool recursive = false)
        {
            var sw = new Stopwatch();
            sw.Start();
            foreach (var f in files)
            {
                Console.WriteLine($@"{Tabs(level)}{f.Name} : {f.MimeType}");
                var s = new MemoryStream();

                if (f.MimeType == GoogleMimeTypes.FolderMimeType)
                {
                    if (recursive)
                        await Dir(await _gdfs.GetChildrenAsync(f), level + 1, true);
                }
            }
            sw.Stop();
            return sw;
        }
    }
}
