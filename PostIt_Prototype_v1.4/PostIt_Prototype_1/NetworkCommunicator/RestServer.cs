using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class RestServer
    {
        private HttpClient _httpClient;

        public async Task<bool> Update(JObject json)
        {
            var result = await _httpClient.PutAsync(_endpoint, JsonContent(json));
            return result.StatusCode == HttpStatusCode.OK;
        }

        private string _owner;
        private string _endpoint => $"https://iwf-vtserv-02.ethz.ch:4003/user/{_owner}/json/";

        public async Task<bool> Insert(JObject json)
        {
            var result = await _httpClient.PostAsync(_endpoint, JsonContent(json));

            return result.StatusCode == HttpStatusCode.OK;
        }


        public async Task<bool> Delete(JObject json)
        {
            throw new NotImplementedException();
        }

        private static StringContent JsonContent(JObject json)
        {
            return new StringContent(
                json.ToString(),
                Encoding.UTF8,
                "application/json");
        }

        public RestServer(string owner)
        {
            _owner = owner;
            _httpClient = new HttpClient();
        }

        public async Task<IEnumerable> Query(object json)
        {
            var result = await _httpClient.GetAsync(_endpoint);
            if (!result.IsSuccessStatusCode)
                return null;
            var jsonResponse = JArray.Parse(await result.Content.ReadAsStringAsync());
            return jsonResponse;
        }
    }
}