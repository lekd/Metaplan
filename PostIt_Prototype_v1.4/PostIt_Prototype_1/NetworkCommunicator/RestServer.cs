using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PostIt_Prototype_1.Utilities;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class RestServer
    {
        private HttpClient _httpClient;

        public async Task<bool> Update(JObject json)
        {
            try
            {
                var result = await _httpClient.PutAsync(Endpoint, JsonContent(json));
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

        //private string Endpoint => $"https://iwf-vtserv-02.ethz.ch:4003/user/{_owner}/";
        private static string Endpoint => $"http://127.0.0.1:4003/";

        public async Task<bool> Insert(JObject json)
        {
            try
            {
                var result = await _httpClient.PostAsync(Endpoint, JsonContent(json));
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

        public async Task<bool> Delete(Dictionary<string, object> keyValues)
        {
            try
            {
                var uriString =
                    (from kv in keyValues select $"{kv.Key}/{kv.Value}").Aggregate((s1, s2) => $"{s1}/{s2}");
                var endPoint = Endpoint + uriString;
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

        private static StringContent JsonContent(JObject json)
        {
            return new StringContent(
                json.ToString(),
                Encoding.UTF8,
                "application/json");
        }

        public RestServer()
        {
            _httpClient = new HttpClient();
        }

        public async Task<JArray> Query(Dictionary<string, object> keyValues)
        {
            try
            {
                var uriString =
                    (from kv in keyValues select $"{kv.Key}/{kv.Value}").Aggregate((s1, s2) => $"{s1}/{s2}");
                var endPoint = Endpoint + uriString;
                var result = await _httpClient.GetAsync(endPoint);
                if (!result.IsSuccessStatusCode)
                    return null;
                var jsonResponse = JArray.Parse(await result.Content.ReadAsStringAsync());
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
    }
}