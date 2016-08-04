using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using File = Dropbox.Api.Files.Metadata;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    /// <summary>
    /// Updates notes from a specific remote folder 
    /// </summary>
    public class NoteUpdater
    {
        #region Public Constructors

        private readonly ICloudFS<File> _storage;
        public NoteUpdater(ICloudFS<File> storage, File folder, string extensionFilter = ".png")
        {
            this._storage = storage;
            this._folder = folder;
            ExtensionFilter = extensionFilter;
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<List<File>> GetUpdatedNotes()
        {
            var childrenFiles = await _storage.GetChildrenAsync(_folder);
            //now process the files
            var updatedNotes = new List<File>();
            foreach (var file in childrenFiles)
            {
                //filter files based on their extension
                if (!file.Name.Contains(ExtensionFilter))
                {
                    continue;
                }

                UpdateNotes(file, updatedNotes);
            }

            return updatedNotes;
        }

        private void UpdateNotes(File file, List<File> updatedNotes)
        {

            var id = GetId(file).GetHashCode();

            //if this file is not existing, then just put it in
            if (!_existingNotes.ContainsKey(id))
            {
                _existingNotes.Add(id, file);
                updatedNotes.Add(file);
            }
            //otherwise we need to check the modification time to see if it's up-to-date or not
            else
            {
                if (GetModifiedTime(file).CompareTo(GetModifiedTime(_existingNotes[id])) > 0)
                {
                    updatedNotes.Add(file);
                    _existingNotes[id] = file;
                }
            }
        }

        private static string GetId(Google.Apis.Drive.v3.Data.File file)
        {
            return file.Id;
        }
        private static string GetId(Dropbox.Api.Files.Metadata file)
        {
            return file.AsFile.Id;
        }

        private static DateTime GetModifiedTime(Dropbox.Api.Files.Metadata file)
        {
            return file.AsFile.ServerModified;
        }

        private static DateTime GetModifiedTime(Google.Apis.Drive.v3.Data.File file)
        {
            return file.ModifiedTime ?? DateTime.Now;
        }

        #endregion Public Methods

        #region Private Methods

        //download all recently-updated image notes and return them together with their corresponding IDs
        public async Task DownloadUpdatedNotes(IEnumerable<File> updatedFileEntries)
        {
            var noteFiles = new Dictionary<int, Stream>();
            foreach (var fileEntry in updatedFileEntries)
            {
                try
                {
                    var sw = new Stopwatch();
                    sw.Restart();
                    // var containingFolder = await Storage.GetFileFromIdAsync(fileEntry.Parents.FirstOrDefault());
                    using (var memStream = new MemoryStream())
                    {
                        await _storage.DownloadFileAsync(fileEntry, memStream);
                        Debug.Write($"{sw.ElapsedMilliseconds}, ");
                        memStream.Seek(0, 0);
                        //extract ID
                        var noteId = GetId(fileEntry).GetHashCode();
                        NewNoteDownloaded?.Invoke(noteId, memStream);
                        noteFiles.Add(noteId, memStream);

                        Debug.WriteLine(sw.ElapsedMilliseconds);
                    }
                    sw.Stop();
                }
                catch (Exception ex)
                {
                    Utilities.UtilitiesLib.LogError(ex);
                }
            }
            var handler = AllNotesDownloaded;
            handler?.Invoke(this, noteFiles);
        }

        #endregion Private Methods

        #region Public Events

        public event EventHandler<Dictionary<int, Stream>> AllNotesDownloaded;

        public event NewNoteStreamsDownloaded NewNoteDownloaded = null;

        #endregion Public Events

        #region Public Properties

        public string ExtensionFilter { get; protected set; }

        #endregion Public Properties

        #region Private Fields

        private readonly Dictionary<int, File> _existingNotes = new Dictionary<int, File>();
        private readonly File _folder;

        #endregion Private Fields



        #region Public Delegates

        public delegate void NewNoteStreamsDownloaded(int noteId, Stream downloadedNoteStream);

        #endregion Public Delegates
    }
}