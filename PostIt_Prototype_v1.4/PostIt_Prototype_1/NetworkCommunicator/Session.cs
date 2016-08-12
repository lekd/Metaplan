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
    using File = Metadata;

    public class Session
    {
        #region Public Constructors

        static Session()
        {
            try
            {
                Storage = new DropboxFS();
                _restServer = new RestServer();
                // Get root folder or create one if null
                RootFolder = Storage.GetFolder(RootFolderName) ??
                             Storage.CreateFolder(RootFolderName);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public Session(string userId, string owner)
        {
            this.UserID = userId;
            this.Owner = owner;

            ParticipantManager = new ParticipantManager(this, _restServer);
        }

        #endregion Public Constructors

        #region Public Methods

        public static async Task<IEnumerable<string>> GetSessionNames(string ownerName)
        {
            /*
            var sessionFolders = await Storage.GetChildrenAsync(RootFolder, GoogleMimeTypes.FolderMimeType);
            return sessionFolders.Select(f => f.UserID).ToList();
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
                { "sessionID", this.UserID },
                { "owner", this.Owner } });
                */
            if (query)
            {
                var json = new JObject
                {
                    ["sessionID"] = this.UserID,
                    ["owner"] = this.Owner,
                    ["participants"] = new JArray()
                };

                if (!await _restServer.Insert(Session.Collection, json))
                    throw new IOException();

                // tests if session already exists
                var temp = await Storage.GetFolderAsync(this.UserID, RootFolder);
                if (temp != null)
                    throw new IOException();
                var sessionFolder = await Storage.CreateFolderAsync(this.UserID, RootFolder);

                Init(sessionFolder);
            }
        }

        public async Task DeleteSessionAsync()
        {
            var r = await _restServer.Delete(Collection, new Dictionary<string, object> { { "owner", Owner }, { "sessionID", UserID } });
        }

        /// <summary>
        /// Creates a new session in the db as well as file server
        /// </summary>
        /// <returns>True if successful, false otherwise. </returns>
        public async Task GetSessionAsync()
        {
            var sessionFolder = await Storage.GetFolderAsync(this.UserID, RootFolder);

            Init(sessionFolder);
        }

        public override string ToString()
        {
            return this.UserID;
        }

        public async Task UpdateNotes()
        {
            var newlyUpdatedNotes = await _stickyNoteUpdater.GetUpdatedNotes();
            await _stickyNoteUpdater.DownloadUpdatedNotes(newlyUpdatedNotes);
        }

        #endregion Public Methods

        #region Private Methods

        /// <summary>
        ///
        /// </summary>
        /// <param UserID="noteId"></param>
        /// <param UserID="downloadedNoteStream"></param>
        private void AnotoNoteUpdater_OnNewNoteDownloaded(int noteId, Stream downloadedNoteStream)
        {
            var text2Str = new Utilities.PointStringToBMP(Properties.Settings.Default.AnotoNoteScale);

            var reader = new StreamReader(downloadedNoteStream);
            var pointsStr = reader.ReadToEnd();
            var bmp = text2Str.FromString(pointsStr,
                Properties.Settings.Default.AnotoNoteInitWidth, Properties.Settings.Default.AnotoNoteInitHeight,
                Properties.Settings.Default.AnotoNoteInitLeft, Properties.Settings.Default.AnotoNoteInitTop);
            var bmpBytes = Utilities.UtilitiesLib.BitmapToBytes(bmp);
            using (var bmpMemStream = new MemoryStream(bmpBytes))
            {
                _noteFiles.Add(noteId, bmpMemStream);
                NewNoteDownloaded?.Invoke(noteId, bmpMemStream);
            }
        }

        private void Init(File sessionFolder)
        {
            _stickyNoteUpdater = new NoteUpdater(Storage, sessionFolder);
            _stickyNoteUpdater.NewNoteDownloaded += StickyNoteUpdater_OnNewNoteDownloaded;
            _anotoNoteUpdater = new NoteUpdater(Storage, sessionFolder, ".txt");
            _anotoNoteUpdater.NewNoteDownloaded += AnotoNoteUpdater_OnNewNoteDownloaded;
        }

        private void StickyNoteUpdater_OnNewNoteDownloaded(int noteId, Stream downloadedNoteStream)
        {
            _noteFiles.Add(noteId, downloadedNoteStream);
            NewNoteDownloaded?.Invoke(noteId, downloadedNoteStream);
        }

        #endregion Private Methods

        #region Public Events

        public event NoteUpdater.NewNoteStreamsDownloaded NewNoteDownloaded;

        #endregion Public Events

        #region Public Properties

        public static File RootFolder { get; private set; }
        public static ICloudFS<File> Storage { get; private set; }
        public string Owner { get; private set; }
        public ParticipantManager ParticipantManager { get; private set; }

        #endregion Public Properties

        #region Public Fields

        public const string Collection = "sessions";
        public const string RootFolderName = "MercoNotes";
        public readonly string UserID;

        #endregion Public Fields

        #region Private Fields

        private static readonly RestServer _restServer;
        private readonly Dictionary<int, Stream> _noteFiles = new Dictionary<int, Stream>();
        private NoteUpdater _anotoNoteUpdater;
        private NoteUpdater _stickyNoteUpdater;

        #endregion Private Fields
    }
}