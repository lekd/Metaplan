using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WhiteboardApp.NetworkCommunicator;
using WhiteboardApp.Utilities;


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
            //await StartDrive();
            //var r = await ValidateToken("eyJhbGciOiJSUzI1NiIsImtpZCI6IjA2MTY3ZGRhODkwNjgxMzE4YzFjNTc3YzdiMjE2YTkzNWI4MzViOTEifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhdWQiOiIxMDEyNjA3NjYxNDM2LTg1MDY2Y2xpaGdqOWw4YXRtYWtkY3ZwMmVqZ21raTd0LmFwcHMuZ29vZ2xldXNlcmNvbnRlbnQuY29tIiwic3ViIjoiMTA5NzgxNzk4NDYyMjY2Mjc3ODk3IiwiZW1haWxfdmVyaWZpZWQiOnRydWUsImF6cCI6IjEwMTI2MDc2NjE0MzYtbXU3ampmNGkzcmtncnM1cDVsbmk3cTdqZ3A4bzE1c3YuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJlbWFpbCI6ImFsaWFsYXZpYUBnbWFpbC5jb20iLCJpYXQiOjE0NzA0MDkwNzksImV4cCI6MTQ3MDQxMjY3OSwibmFtZSI6IkFsaSBBbGF2aSIsImdpdmVuX25hbWUiOiJBbGkiLCJmYW1pbHlfbmFtZSI6IkFsYXZpIiwibG9jYWxlIjoiZW4ifQ.G6Cr6AElOBwJMyhHBUUZtqnO95XFUuKLbgJHVxioEaGTCt5rktQsBbZYo1vzhVAFk6SIsLV3MZz4r4z6DPJdO9muyLBzwUrDoZYfowabzfks82jIecihlegzehZhwXhgbuiuMAO5bfBpxm6y3-srHsDRoGaYGJiU0BcdlmDjQTowkcJhk-ZvzCozmzL4LmVUUr6YN1vZTGPUmuAO3UudwKVAScDwMnbd8XFtMBCVo8vZNmC6pHVRoyj55j36M5GgkyxPRBaIulyuz3Pdttx0BBAKGGoEfRJC7vMDNl1dA3OB4x4sRJctTvRSkWfNElCr1DF0pqw148Ag8HXEK2Eqbg");
            //Console.WriteLine(r);
        }


        /*
        private static async Task StartDrive()
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
        */
    }
}
