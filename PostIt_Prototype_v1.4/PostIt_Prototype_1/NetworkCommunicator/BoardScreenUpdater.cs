using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Drive.v3;
using File = Google.Apis.Drive.v3.Data.File;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    /// <summary>
    /// TODO: Make it Thread-safe Singleton
    /// </summary>
    class BoardScreenUpdater
    {
        private static volatile BoardScreenUpdater instance;
        private static object syncRoot = new Object();
        private GoogleDriveFS Storage;
        private static File screenShotFolder;

        private BoardScreenUpdater(GoogleDriveFS storage)
        {
            this.Storage = storage;
        }

        public static async Task<BoardScreenUpdater> GetInstance(GoogleDriveFS storage)
        {

            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new BoardScreenUpdater(storage);
                }
                screenShotFolder = await storage.GetFolderAsync("ScreenShots");
            }
            return instance;
        }
        public async Task UpdateMetaplanBoardScreen(MemoryStream screenshotStream, int retry = 3)
        {
            if (screenShotFolder == null)
                return;
            
            try
            {
                await Storage.UploadFileAsync(screenshotStream,
                    "MetaplanBoard_CELTIC.png",
                    screenShotFolder);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                // if (retry > 0)
                //   UpdateMetaplanBoardScreen(screenshotStream, retry - 1);
            }

        }

        public async void UpdateMetaplanBoardScreen(byte[] screenshotBytes)
        {
            return;
            try
            {
                using (var stream = new MemoryStream(screenshotBytes))
                {
                    await UpdateMetaplanBoardScreen(stream);
                }
            }
            catch (Exception ex)
            {
                Debugger.Break();
                Utilities.UtilitiesLib.LogError(ex);
            }
        }
    }
}
