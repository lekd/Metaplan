using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Google.Apis.Drive.v3;

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
        private BoardScreenUpdater(GoogleDriveFS storage)
        {
            this.Storage = storage;
        }

        public static BoardScreenUpdater GetInstance(GoogleDriveFS storage)
        {

            if (instance == null)
            {
                lock (syncRoot)
                {
                    if (instance == null)
                        instance = new BoardScreenUpdater(storage);
                }
            }
            return instance;
        }
        public async void UpdateMetaplanBoardScreen(MemoryStream screenshotStream, int retry = 3)
        {
            //Thread.Sleep(1000);
            try
            {

                await Storage.UploadFileAsync(screenshotStream,
                    "MetaplanBoard_CELTIC.png",
                    await Storage.GetFolderAsync("ScreenShots"));
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                // if (retry > 0)
                //   UpdateMetaplanBoardScreen(screenshotStream, retry - 1);
            }

        }

        public void UpdateMetaplanBoardScreen(byte[] screenshotBytes)
        {
            return;
            try
            {
                using (MemoryStream stream = new MemoryStream(screenshotBytes))
                {
                    UpdateMetaplanBoardScreen(stream);
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
