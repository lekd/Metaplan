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
        private static volatile BoardScreenUpdater _instance;
        private static readonly object SyncRoot = new object();
        private readonly GoogleDriveFS _storage;
        private static File _screenShotFolder;

        private BoardScreenUpdater(GoogleDriveFS storage)
        {
            this._storage = storage;
        }

        public static BoardScreenUpdater GetInstance(GoogleDriveFS storage, File screenshotFolder)
        {

            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new BoardScreenUpdater(storage);
                }
                _screenShotFolder = screenshotFolder;
            }
            return _instance;
        }
        public async Task UpdateMetaplanBoardScreen(MemoryStream screenshotStream, int retry = 3)
        {
            if (_screenShotFolder == null)
                return;
            
            try
            {
                await _storage.UploadFileAsync(screenshotStream,
                    "MetaplanBoard_CELTIC.png",
                    _screenShotFolder);
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
            /*
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
            }*/
        }
    }
}
