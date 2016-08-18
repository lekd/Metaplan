using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IdentityModel.Selectors;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Compilation;
using System.Web.Routing;
using System.Web.SessionState;
using Newtonsoft.Json;
using System.Net.Http;
using Google;
using Newtonsoft.Json.Linq;

namespace WhiteboardApp.NetworkCommunicator
{
    public class TokenVerifier
    {
        // Validate token according to https://developers.google.com/identity/sign-in/android/backend-auth
        public static readonly string[] ValidClientIDs = { "1012607661436-4d0j7s8br9311ict7pg25uj833jrkrni.apps.googleusercontent.com" };

        public static async Task<string> ValidateToken(string token)
        {
            var client = new HttpClient();
            string endpoint = $"https://www.googleapis.com/oauth2/v3/tokeninfo?id_token={token}";
            var result = await client.GetAsync(endpoint);
            Debug.WriteLine(result);
            if (result.StatusCode != HttpStatusCode.OK)
                throw new HttpException("Server returned error!");

            var jsonResponse = JObject.Parse(await result.Content.ReadAsStringAsync());
            if (!ValidClientIDs.Contains(jsonResponse["aud"].ToString()))
                throw new HttpException("User not authorized for this app!");

            return jsonResponse["email"].ToString();
        }
    }
}
