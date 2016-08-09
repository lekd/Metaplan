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

namespace PostIt_Prototype_1.NetworkCommunicator
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

        public async Task<bool> AddParticipant(string participantTokenID)
        {
            if (await TokenVerifier.ValidateToken(participantTokenID) == null)
                return false;
            dynamic json = new JObject();
            dynamic query = new JObject();
            query.sessionID = Session.Name;
            dynamic updates = new JObject(string.Format("{$addToSet : { participants : \"{0}\"}}", participantTokenID));

            json.query = query;
            json.updates = updates;

            return await _restServer.Update(json);
        }


        public string SessionOwner { get; set; }

        public async Task<IEnumerable<string>> GetParticipants()
        {
            var json = new JObject();
            json["sessionID"] = Session.Name;
            json["owner"] = Session.Owner;

            // check if session is unique
            var query = await _restServer.Query(json);

            var r = new JArray(json["participants"]);
            var result = new List<String>(r.Count);
            foreach (var e in r)
                result.Add(e.Value<string>());
            return result;
        }
    }
}
