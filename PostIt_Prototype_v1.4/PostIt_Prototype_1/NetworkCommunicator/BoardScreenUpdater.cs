using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Google.Apis.Drive.v3;
using File = Google.Apis.Drive.v3.Data.File;

namespace WhiteboardApp.NetworkCommunicator
{
    /// <summary>
    /// TODO: Make it Thread-safe Singleton
    /// </summary>
    class BoardScreenUpdater
    {
        private static volatile BoardScreenUpdater _instance;
        private static readonly object SyncRoot = new object();

        private static RemoteFile _screenShotFolder;

        private BoardScreenUpdater()
        {

        }

        public static BoardScreenUpdater GetInstance(RemoteFile screenshotFolder)
        {

            if (_instance == null)
            {
                lock (SyncRoot)
                {
                    if (_instance == null)
                        _instance = new BoardScreenUpdater();
                }
                _screenShotFolder = screenshotFolder;
            }
            return _instance;
        }
        public void UpdateMetaplanBoardScreen(MemoryStream screenshotStream, int retry = 3)
        {
            throw new NotImplementedException();
            /*
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
            */

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
