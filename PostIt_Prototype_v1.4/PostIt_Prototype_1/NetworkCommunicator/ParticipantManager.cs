using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Newtonsoft.Json.Linq;

namespace WhiteboardApp.NetworkCommunicator
{
    public class ParticipantManager
    {
        // How authentication works:
        // In order to ensure all the participants are authenticated, the moderator adds a list of participants 
        // who can access the session using their Gmail accounts.
        // The participants who want to join also type in the Gmail account of the moderator on their client apps
        // The root session folder is named as the moderator's Google ID.
        // 
        // When a participant creates and uploads a file, her Google ID is also attached to the file as a header,
        // and it is inserted into the session only if he or she is on the list.

        public ParticipantManager(Session session, RestServer restServer)
        {
            this.Session = session;
            this._restServer = restServer;
        }

        public Session Session { get; }

        public string SessionId;
        private readonly RestServer _restServer;

        public async Task<bool> AddParticipant(string participantEmail)
        {
            var query = new JObject { ["sessionID"] = Session.UserID };
            var updates = new JObject { ["$addToSet"] = new JObject() { ["participants"] = participantEmail } };
            var json = new JObject
            {
                ["query"] = query,
                ["updates"] = updates
            };

            return await _restServer.Update(Collection, json);
        }


        public string SessionOwner { get; set; }
        public const string Collection = "documents";
        public async Task<IEnumerable<string>> GetParticipants()
        {
            var result = new List<string>();
            var json = new JObject
            {
                ["sessionID"] = Session.UserID,
                ["owner"] = Session.Owner
            };

            // check if session is unique
            var query = (await _restServer.Query(Collection, new Dictionary<string, object>
            {
                {"sessionID", Session.UserID},
                {"owner", Session.Owner}
            }));

            if (query != null && query.Count > 0)
            {
                var r = query.First();
                var participants = r["participants"];

                foreach (var e in participants)
                    result.Add(e.Value<string>());
            }
            return result;
        }
    }
}
