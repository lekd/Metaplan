using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using WhiteboardApp.Utilities;

namespace WhiteboardApp.NetworkCommunicator
{
    public class RestServer
    {
        #region Public Constructors

        public RestServer()
        {
            _httpClient = new HttpClient();
        }

        #endregion Public Constructors

        #region Public Methods

        public async Task<bool> Delete(string collection, Dictionary<string, object> keyValues)
        {
            try
            {
                var uriString =
                    (from kv in keyValues select $"{kv.Key}/{kv.Value}").Aggregate((s1, s2) => $"{s1}/{s2}");
                var endPoint = $"{Endpoint}/{collection}/{uriString}";

                var result = await _httpClient.DeleteAsync(endPoint);
                return result.IsSuccessStatusCode;
            }
            catch (HttpRequestException ex)
            {
                var e = ex.InnerException as WebException;
                if (e != null && e.Status == WebExceptionStatus.ConnectFailure)
                    UtilitiesLib.TerminateWithError(ex.InnerException.Message);
            }
            return false;
        }

        public async Task<bool> Insert(string collection, JObject json)
        {
            try
            {
                var endPoint = $"{Endpoint}/{collection}";
                var result = await _httpClient.PostAsync(endPoint, JsonContent(json));
                return result.StatusCode == HttpStatusCode.OK;
            }
            catch (HttpRequestException ex)
            {
                var e = ex.InnerException as WebException;
                if (e != null && e.Status == WebExceptionStatus.ConnectFailure)
                    UtilitiesLib.TerminateWithError(ex.InnerException.Message);
            }
            return false;
        }

        public async Task<JArray> Query(string collection, Dictionary<string, object> keyValues)
        {
            try
            {
                var uriString = (keyValues == null || keyValues.Count == 0) ?
                    ""
                    :
                    (from kv in keyValues
                     select $"{kv.Key}/{kv.Value}").Aggregate((s1, s2) => $"{s1}/{s2}");

                var endPoint = $"{Endpoint}/{collection}/{uriString}";
                var result = await _httpClient.GetAsync(endPoint);
                if (!result.IsSuccessStatusCode)
                    return null;
                var stringResponse = await result.Content.ReadAsStringAsync();
                var jsonResponse = JArray.Parse(stringResponse);
                return jsonResponse;
            }
            catch (HttpRequestException ex)
            {
                var e = ex.InnerException as WebException;
                if (e != null && e.Status == WebExceptionStatus.ConnectFailure)
                    UtilitiesLib.TerminateWithError(ex.InnerException.Message);
            }
            return null;
        }

        public async Task<bool> Update(string collection, JObject json)
        {
            try
            {
                var endPoint = $"{Endpoint}/{collection}";
                var result = await _httpClient.PutAsync(endPoint, JsonContent(json));
                return result.StatusCode == HttpStatusCode.OK;
            }
            catch (HttpRequestException ex)
            {
                var e = ex.InnerException as WebException;
                if (e != null && e.Status == WebExceptionStatus.ConnectFailure)
                    UtilitiesLib.TerminateWithError(ex.InnerException.Message);
            }
            return false;
        }

        #endregion Public Methods

        #region Private Methods

        private static StringContent JsonContent(JObject json)
        {
            return new StringContent(
                json.ToString(),
                Encoding.UTF8,
                "application/json");
        }

        #endregion Private Methods

        #region Private Properties

        //private string Endpoint => $"https://iwf-vtserv-02.ethz.ch:4003/user/{_owner}/";
        private static string Endpoint => $"http://127.0.0.1:4003";

        #endregion Private Properties

        #region Private Fields

        private HttpClient _httpClient;

        #endregion Private Fields
    }
}