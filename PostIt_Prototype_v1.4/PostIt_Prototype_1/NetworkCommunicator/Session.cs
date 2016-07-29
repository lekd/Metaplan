using System;
using System.Collections.Generic;

//using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Threading.Tasks;

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
                //InitFolderIfNecessary().Wait();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public Session(string name)
        {
            this.Name = name;
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

        public async Task CreateSession()
        {
            var rootFolder = await GetOrCreateRootFolder();

            // tests if session already exists
            var temp = await Storage.GetFolderAsync(this.Name, rootFolder);
            if (temp != null)
                throw new IOException();
            var sessionFolder = await Storage.CreateFolderAsync(this.Name, rootFolder);

            Init(sessionFolder);
        }

        public async Task GetSession()
        {
            var rootFolder = await GetOrCreateRootFolder();
            var sessionFolder = await Storage.GetFolderAsync(this.Name, rootFolder);

            Init(sessionFolder);
        }

        public async Task UpdateNotes()
        {
            var newlyUpdatedNotes = await StickyNoteUpdater.GetUpdatedNotes();
            await StickyNoteUpdater.DownloadUpdatedNotes(newlyUpdatedNotes);
        }

        #endregion Public Methods

        #region Private Methods

        private static async Task<File> GetOrCreateRootFolder()
        {
            var rootFolder = await Storage.GetFolderAsync(RootFolderName);
            if (rootFolder == null)
                rootFolder = await Storage.CreateFolderAsync(RootFolderName);
            return rootFolder;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="noteId"></param>
        /// <param name="downloadedNoteStream"></param>
        private void AnotoNoteUpdaterNewNoteDownloaded(int noteId, Stream downloadedNoteStream)
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
            }
        }

        private void Init(File sessionFolder)
        {
            StickyNoteUpdater = new NoteUpdater(Storage, sessionFolder);
            AnotoNoteUpdater = new NoteUpdater(Storage, sessionFolder, ".txt");
            AnotoNoteUpdater.NewNoteDownloaded += AnotoNoteUpdaterNewNoteDownloaded;
        }

        #endregion Private Methods

        #region Public Properties

        public static GoogleDriveFS Storage { get; private set; }

        #endregion Public Properties

        #region Private Fields

        private const string RootFolderName = "MercoNotes";
        private Dictionary<int, Stream> _noteFiles = new Dictionary<int, Stream>();
        private NoteUpdater AnotoNoteUpdater;
        private string Name;
        private NoteUpdater StickyNoteUpdater;

        #endregion Private Fields

        //private ICloudStorageAccessToken storageToken;
        // private readonly CloudStorage _driveService;
    }
}