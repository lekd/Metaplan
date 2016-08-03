using System;
using System.Collections.Generic;

//using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using PostIt_Prototype_1.Utilities;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    using File = Google.Apis.Drive.v3.Data.File;

    public class Session
    {
        #region Public Constructors

        static Session()
        {
            try
            {
                Storage = new GoogleDriveFS();

                // Get root folder
                RootFolder = Storage.GetFolder(RootFolderName);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public override string ToString()
        {
            return this._name;
        }

        public static async Task<IEnumerable<string>> GetSessionNames()
        {
            var sessionFolders = await Storage.GetChildrenAsync(RootFolder, GoogleMimeTypes.FolderMimeType);
            return sessionFolders.Select(f => f.Name).ToList();
        }
        public Session(string name)
        {
            this._name = name;
        }

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

        public async Task CreateSessionAsync()
        {
            // tests if session already exists
            var temp = await Storage.GetFolderAsync(this._name, RootFolder);
            if (temp != null)
                throw new IOException();
            var sessionFolder = await Storage.CreateFolderAsync(this._name, RootFolder);

            Init(sessionFolder);
        }

        public async Task GetSessionAsync()
        {
            var sessionFolder = await Storage.GetFolderAsync(this._name, RootFolder);

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

        public static GoogleDriveFS Storage { get; private set; }

        #endregion Public Properties

        #region Private Fields

        private const string RootFolderName = "MercoNotes";
        private readonly Dictionary<int, Stream> _noteFiles = new Dictionary<int, Stream>();
        private NoteUpdater _anotoNoteUpdater;
        private readonly string _name;
        private NoteUpdater _stickyNoteUpdater;
        public static File RootFolder { get; private set; }

        #endregion Private Fields
    }
}