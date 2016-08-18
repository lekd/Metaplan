using System;
using System.Collections;
using System.Collections.Generic;

//using AppLimit.CloudComputing.SharpBox;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Documents;
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
            if (r == null)
                return new List<string>();
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
        public async Task<ICollection> GetSessionAsync()
        {

            var session = await
                _restServer.Query(Collection,
                    new Dictionary<string, object> { { "sessionID", this.sessionID }, { "owner", this.Owner } });

            return session.ToList();
        }

        public override string ToString()
        {
            return this.sessionID;
        }

        public async Task UpdateNotes()
        {
            _updatedNotes = await GetUpdatedNotes();
            foreach (var e in _updatedNotes)
                NewNoteDownloaded?.Invoke(e.Name.GetHashCode(), new MemoryStream(e.Content));
        }

        private string lastTimeStamp = null;
        private async Task<List<RemoteFile>> GetUpdatedNotes()
        {
            var query = new Dictionary<string, object> { { "sessionID", sessionID }, { "owner", Owner } };
            if (lastTimeStamp != null)
                query.Add("lastTimeStamp", lastTimeStamp);

            var r = await _restServer.Query("sessions", query);
            if (r == null || r.Count == 0 || r[0] == null || r[0]["files"] == null)
            {
                return new List<RemoteFile>();
            }
            var session = r[0];
            var list = (from e in session["files"] select new RemoteFile(e)).ToList();

            lastTimeStamp = list.Max(e => e.ModifiedTime);
            return list;
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

        private async Task UpdateSession(JObject jObject)
        {
            var json = new JObject
            {
                ["query"] = new JObject
                {
                    ["owner"] = Owner,
                    ["sessionID"] = sessionID
                }
                ,
                ["updates"] = jObject
            };
            await _restServer.Update(Collection, json);
        }


        private async Task AddFile(Stream screenshotStream, string fileFamily, string name)
        {
            var bytes = new byte[screenshotStream.Length];
            screenshotStream.Read(bytes, 0, bytes.Length);
            var updates = new JObject
            {
                ["$addToSet"] =
                new JObject
                {
                    ["files"] = new JObject
                    {
                        ["type"] = fileFamily,
                        ["content"] = bytes,
                        ["name"] = name,
                        ["modifiedDate"] = ""
                    }
                }
            };
            await UpdateSession(updates);
        }

        public async Task UploadScreenShotAsync(Stream screenshotStream)
        {
            await AddFile(screenshotStream, "screenShots", "screenShot" + DateTime.Now);
        }


        public async Task UploadNoteAsync(Stream screenshotStream)
        {
            await AddFile(screenshotStream, "notes", "note" + DateTime.Now);
        }
    }
}