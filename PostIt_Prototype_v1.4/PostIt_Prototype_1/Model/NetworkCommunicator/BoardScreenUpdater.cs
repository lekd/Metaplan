using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using AppLimit.CloudComputing.SharpBox;
using MongoDB.Bson;
using PostIt_Prototype_1.Model.Database;

namespace PostIt_Prototype_1.Model.NetworkCommunicator
{
    /// <summary>
    /// TODO: Make it Thread-safe Singleton
    /// </summary>
    class BoardScreenUpdater
    {
        private static volatile BoardScreenUpdater _instance;
        private static object _syncRoot = new Object();
        private MongoStorage _storage;
        private BoardScreenUpdater(MongoStorage storage)
        {
            this._storage = storage;
        }

        public static BoardScreenUpdater GetInstance (MongoStorage storage)
        {

                if (_instance == null)
                {
                    lock (_syncRoot)
                    {
                        if (_instance == null)
                            _instance = new BoardScreenUpdater(storage);
                    }
                }
            return _instance;
        }
        public void UpdateMetaplanBoardScreen(MemoryStream screenshotStream, int retry = 3)
        {

            Thread.Sleep(1000);
            BsonDocument ice;
            try
            {
                var session = _storage.GetSession("/");
                ice = _storage.UploadFile(screenshotStream, "MetaplanBoard_CELTIC.png", session);
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
                using (var stream = new MemoryStream(screenshotBytes))
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
