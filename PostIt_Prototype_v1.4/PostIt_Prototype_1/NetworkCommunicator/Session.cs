using System;
using System.Collections.Generic;

//using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using Dropbox.Api.Files;
using Newtonsoft.Json.Linq;
using PostIt_Prototype_1.Utilities;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    using File = Metadata;

    public class Session
    {
        #region Public Constructors
        public const string RootFolderName = "MercoNotes";

        static Session()
        {
            try
            {
                Storage = new DropboxFS();

                // Get root folder or create one if null
                RootFolder = Storage.GetFolder(RootFolderName) ??
                             Storage.CreateFolder(RootFolderName);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public override string ToString()
        {
            return this.Name;
        }

        public static async Task<IEnumerable<string>> GetSessionNames()
        {
            var sessionFolders = await Storage.GetChildrenAsync(RootFolder, GoogleMimeTypes.FolderMimeType);
            return sessionFolders.Select(f => f.Name).ToList();
        }
        public Session(string name, string owner)
        {
            this.Name = name;
            this.Owner = owner;
            _restServer = new RestServer(owner);
            ParticipantManager = new ParticipantManager(this, _restServer);
        }

        public ParticipantManager ParticipantManager { get; private set; }

        public string Owner { get; private set; }

        #endregion Public Constructors

        #region Public Methods

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

        /// <summary>
        /// Creates a new session in the db as well as file server
        /// </summary>
        /// <returns>True if successful, false otherwise. </returns>

        public async Task CreateSessionAsync()
        {

            var json = new JObject
            {
                ["sessionID"] = this.Name,
                ["owner"] = this.Owner
            };

            // check if session is unique
            var query = await _restServer.Query(json);
            
            json["participants"] = new JArray();
                    
            if (!await _restServer.Insert(json))
                throw new IOException();

            // tests if session already exists
            var temp = await Storage.GetFolderAsync(this.Name, RootFolder);
            if (temp != null)
                throw new IOException();
            var sessionFolder = await Storage.CreateFolderAsync(this.Name, RootFolder);

            Init(sessionFolder);
        }

        public async Task GetSessionAsync()
        {
            var sessionFolder = await Storage.GetFolderAsync(this.Name, RootFolder);

            Init(sessionFolder);
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
        /// <param name="noteId"></param>
        /// <param name="downloadedNoteStream"></param>
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

        public event NoteUpdater.NewNoteStreamsDownloaded NewNoteDownloaded;
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

        #region Public Properties

        public static ICloudFS<File> Storage { get; private set; }

        #endregion Public Properties

        #region Private Fields

        private readonly Dictionary<int, Stream> _noteFiles = new Dictionary<int, Stream>();
        private NoteUpdater _anotoNoteUpdater;
        public readonly string Name;
        private NoteUpdater _stickyNoteUpdater;
        private RestServer _restServer;
        public static File RootFolder { get; private set; }

        #endregion Private Fields
    }
}