using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Util.Store;
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

        private static string CredPath()
        {
            var credPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            credPath = Path.Combine(credPath, ".credentials/metaplan.json");
            return credPath;
        }

        public static async Task<bool> SignIn()
        {
            return await SignIn(true);
        }

        public static string Owner { get; private set; }
        private static async Task<bool> SignIn(bool canTryAgain)
        {
            var result = await ValidateUser();
            if (result != "INVALID")
            {
                Owner = result;
                return true;
            }

            if (canTryAgain)
            {
                // IdToken is expired
                Directory.Delete(CredPath(), true);
                return await SignIn(true);
            }
            else
                return false;

        }

        private static async Task<string> ValidateUser()
        {
            using (var stream =
                new FileStream("client_secret.json", FileMode.Open, FileAccess.Read))
            {
                var credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    new[] { "profile", "email" },
                    "user",
                    CancellationToken.None,
                    new FileDataStore(CredPath(), true)).Result;
                Trace.WriteLine(credential.UserId);
                Trace.WriteLine(credential.Token.IdToken);                
                var restServer = new RestServer();
                var result = await restServer.Query("verify", new Dictionary<string, object>
                {
                    {"id_token", $"{credential.Token.IdToken}"}
                });

                if (result == null)
                {
                    MessageBox.Show("Error logging in. Please reopen the application.");
                    Environment.Exit(-1);
                }

                return result[0]["response"].ToString();
            }
        }

        public static async Task<string> GetUser(string IdToken)
        {
            var client = new HttpClient();
            string endpoint = $"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={IdToken}";
            var result = await client.GetAsync(endpoint);


            var jsonResponse = JObject.Parse(await result.Content.ReadAsStringAsync());

            return jsonResponse["email"].ToString();
        }
        public Session Session { get; }

        public string SessionId;
        private readonly RestServer _restServer;
        public async Task<bool> CreateUser(string name)
        {
            var query = new JObject { ["userName"] = name };
            return await _restServer.Insert(Collection, query);
        }
        public async Task<bool> AddParticipant(string participantEmail)
        {
            var query = new JObject { ["sessionID"] = Session.sessionID };
            var updates = new JObject { ["$addToSet"] = new JObject() { ["participants"] = participantEmail } };
            var json = new JObject
            {
                ["query"] = query,
                ["updates"] = updates
            };

            return await _restServer.Update(Collection, json);
        }


        public string SessionOwner { get; set; }
        public const string Collection = "sessions";
        public async Task<IEnumerable<string>> GetParticipants()
        {
            var result = new List<string>();
            var json = new JObject
            {
                ["sessionI"] = Session.sessionID,
                ["owner"] = Session.Owner
            };

            // check if session is unique
            var query = (await _restServer.Query(Collection, new Dictionary<string, object>
            {
                {"sessionID", Session.sessionID},
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
