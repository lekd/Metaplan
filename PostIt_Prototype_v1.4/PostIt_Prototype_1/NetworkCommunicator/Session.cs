using System;
using System.Collections.Generic;

//using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Dropbox.Api.Files;
using Newtonsoft.Json.Linq;

namespace WhiteboardApp.NetworkCommunicator
{
    public class Session
    {
        #region Public Constructors

        static Session()
        {
            try
            {
                //Storage = new LiuxFS();
                _restServer = new RestServer();
                // Get root folder or create one if null
                //RootFolder = Storage.GetFolder(RootFolderName) ??
                // Storage.CreateFolder(RootFolderName);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public Session(string sessionId, string owner)
        {
            this.sessionID = sessionId;
            this.Owner = owner;

            ParticipantManager = new ParticipantManager(this, _restServer);
            _stickyNoteUpdater = new NoteUpdater(this, _restServer);
        }

        #endregion Public Constructors

        #region Public Methods

        public static async Task<IEnumerable<string>> GetSessionNames(string ownerName)
        {
            /*
            var sessionFolders = await Storage.GetChildrenAsync(RootFolder, GoogleMimeTypes.FolderMimeType);
            return sessionFolders.Select(f => f.sessionID).ToList();
            */
            var r = await _restServer.Query(Collection, new Dictionary<string, object> { { "owner", ownerName } });
            return (from e in r select e["sessionID"].ToString());
        }

        public void Close()
        {
            try
            {
                //_driveService.Close();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public async Task CreateSessionAsync()
        {
            // check if session is unique
            var query = true;
            /*await _restServer.Query(Collection, new Dictionary<string, object> {
                { "sessionID", this.sessionID },
                { "owner", this.Owner } });
                */
            if (query)
            {
                var json = new JObject
                {
                    ["sessionID"] = this.sessionID,
                    ["owner"] = this.Owner,
                    ["participants"] = new JArray()
                };

                if (!await _restServer.Insert(Session.Collection, json))
                    throw new IOException();
            }
        }

        public async Task DeleteSessionAsync()
        {
            var r = await _restServer.Delete(Collection, new Dictionary<string, object> { { "owner", Owner }, { "sessionID", sessionID } });
        }


        /// <summary>
        /// Creates a new session in the db as well as file server
        /// </summary>
        /// <returns>True if successful, false otherwise. </returns>
        public async Task GetSessionAsync()
        {
            /*
            var files = await
                _restServer.Query("files",
                    new Dictionary<string, object> { { "sessionID", this.sessionID }, { "owner", this.Owner } });*/
        }

        public override string ToString()
        {
            return this.sessionID;
        }

        public async Task UpdateNotes()
        {
            _updatedNotes = await _stickyNoteUpdater.GetUpdatedNotes();
            foreach (var e in _updatedNotes)
                NewNoteDownloaded?.Invoke(e.Name.GetHashCode(), new MemoryStream(e.Content));
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        ///
        /// </summary>
        /// <param sessionID="noteId"></param>
        /// <param sessionID="downloadedNoteStream"></param>
        private void ProcessAnoto(RemoteFile anotoFile)
        {
            var text2Str = new Utilities.PointStringToBMP(Properties.Settings.Default.AnotoNoteScale);

            var pointsStr = System.Text.Encoding.UTF8.GetString(anotoFile.Content);
            var bmp = text2Str.FromString(pointsStr,
                Properties.Settings.Default.AnotoNoteInitWidth, Properties.Settings.Default.AnotoNoteInitHeight,
                Properties.Settings.Default.AnotoNoteInitLeft, Properties.Settings.Default.AnotoNoteInitTop);
            var bmpBytes = Utilities.UtilitiesLib.BitmapToBytes(bmp);
            using (var bmpMemStream = new MemoryStream(bmpBytes))
            {
                anotoFile.Content = bmpMemStream.ToArray();
            }
        }



        #endregion Private Methods

        #region Public Events

        #endregion Public Events

        #region Public Properties

        public static RemoteFile RootFolder { get; private set; }
        //public static ICloudFS<File> Storage { get; private set; }
        public string Owner { get; private set; }
        public ParticipantManager ParticipantManager { get; private set; }

        #endregion Public Properties

        #region Public Fields

        public const string Collection = "sessions";
        public const string RootFolderName = "MercoNotes";
        public readonly string sessionID;

        #endregion Public Fields

        #region Private Fields

        private static readonly RestServer _restServer;
        //private NoteUpdater _anotoNoteUpdater;
        private NoteUpdater _stickyNoteUpdater;
        private List<RemoteFile> _updatedNotes;

        #endregion Private Fields

        public event Action<int, Stream> NewNoteDownloaded;

        public async Task UploadScreenShotAsync(MemoryStream screenshotStream)
        {
            var bytes = new byte[screenshotStream.Length];
            screenshotStream.Read(bytes, 0, bytes.Length);
            await _restServer.Insert("files", new JObject { "content", bytes });
        }
    }
}