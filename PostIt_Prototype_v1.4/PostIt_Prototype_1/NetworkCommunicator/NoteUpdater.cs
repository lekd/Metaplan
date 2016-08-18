using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using File = WhiteboardApp.NetworkCommunicator.RemoteFile;

namespace WhiteboardApp.NetworkCommunicator
{
    /// <summary>
    /// Updates notes from a specific remote folder
    /// </summary>
    public class NoteUpdater
    {
        #region Public Constructors

        public NoteUpdater(Session session, RestServer restServer, string extensionFilter = ".png")
        {
            _session = session;
            _restServer = restServer;
            ExtensionFilter = extensionFilter;
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<List<File>> GetUpdatedNotes()
        {
            var query = new Dictionary<string, object> { { "sessionID", _session.sessionID }, { "owner", _session.Owner } };
            if (lastTimeStamp != null)
                query.Add("lastTimeStamp", lastTimeStamp);

            var r = await _restServer.Query("files", query);
            if (r == null)
            {
                return new List<RemoteFile>();
            }
            var list = (from e in r select new RemoteFile(e)).ToList();

            lastTimeStamp = list.Max(e => e.ModifiedTime);
            return list;
        }

        #endregion Public Methods

        #region Public Properties

        public string ExtensionFilter { get; protected set; }

        #endregion Public Properties

        #region Private Fields

        private readonly RestServer _restServer;
        private readonly Session _session;
        private string lastTimeStamp = null;

        #endregion Private Fields
    }
}