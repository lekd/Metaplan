using System;
using System.Collections.Generic;
using System.IO;
using AppLimit.CloudComputing.SharpBox;
using MongoDB.Bson;
using PostIt_Prototype_1.Model.Database;
using PostIt_Prototype_1.NetworkCommunicator;
//using AppLimit.CloudComputing.SharpBox;

namespace PostIt_Prototype_1.Model.NetworkCommunicator
{
    public class NoteUpdater
    {
        #region Public Constructors
        static NoteUpdater()
        {

        }
        public NoteUpdater(string searchPattern = ".png")
        {
            SearchPattern = searchPattern;
            try
            {
                Storage = new MongoStorage();
          

                InitFolderIfNecessary();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            //_existingNotes = new Dictionary<int, ICloudFileSystemEntry>();
        }

        #endregion Public Constructors

        #region Public Methods

        public void Close()
        {
            try
            {
                Storage.Close();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
        }

        public void UpdateNotes()
        {
            //IEnumerable<ICloudFileSystemEntry> newlyUpdatedNotes = 
            DownloadUpdatedNotes(GetUpdatedNotes(DataFolder, SearchPattern));
        }

        #endregion Public Methods

        #region Private Methods

        //download all recently-updated image notes and return them together with their corresponding IDs
        private void DownloadUpdatedNotes(IEnumerable<BsonDocument> updatedFileEntries)
        {
            foreach (var fileEntry in updatedFileEntries)
            {
                try
                {
                    var noteFiles = new Dictionary<int, Stream>();
                    //TODO: Make storage truly generic 
                    var containingFolder = _session;
                    using (var memStream = new MemoryStream())
                    {
                        
                        Storage.DownloadFile(fileEntry["name"].AsString, containingFolder, memStream);
                        memStream.Seek(0, 0);

                        //extract ID
                        var noteId = GetIDfromFileName(fileEntry["name"].AsString);
                        
                        ProcessMemStream(noteFiles, memStream, (int)noteId);
                    }

                    NoteStreamsDownloadedHandler?.Invoke(noteFiles);
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }
        }

        protected virtual void ProcessMemStream(Dictionary<int, Stream> noteFiles, MemoryStream memStream, int noteId)
        {
            noteFiles.Add(noteId, memStream);
        }

        private static long GetIDfromFileName(string fileName)
        {
            var nameComponents = fileName.Split(new string[] { "." }, StringSplitOptions.RemoveEmptyEntries);
            if (nameComponents.Length != 2)
            {
                return -1;
            }
            try
            {
                return long.Parse(nameComponents[0]).GetHashCode();
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                return -1;
            }
        }

        // Get notes (except Anoto notes)
        private IEnumerable<BsonDocument> GetUpdatedNotes(string sessionName, string extensionFilter = ".png")
        {
            var updatedNotes = new List<BsonDocument>();
            
            try
            {
                foreach (var bsonValue in Storage.GetSession(sessionName)["notes"].AsBsonArray)                
                    updatedNotes.Add(bsonValue.AsBsonDocument);                
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                return updatedNotes;
            }            
            return updatedNotes;
        }

        private void InitFolderIfNecessary()
        {
            try
            {
                Storage.GetSession(DataFolder);
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
                Storage.CreateSession(DataFolder);
            }
        }

        #endregion Private Methods

        #region Public Events

        public event NewNoteStreamsDownloaded NoteStreamsDownloadedHandler = null;

        #endregion Public Events

        #region Public Properties

        public MongoStorage Storage { get; private set; }
        public string SearchPattern { get; protected set; }

        #endregion Public Properties

        #region Private Fields

        private const string DataFolder = "/CELTIC_Notes";

        private readonly Dictionary<int, ICloudFileSystemEntry> _existingNotes = null;
        private BsonDocument _session;

        #endregion Private Fields
    }
    public delegate void NewNoteStreamsDownloaded(Dictionary<int, Stream> downloadedNoteStream);

}