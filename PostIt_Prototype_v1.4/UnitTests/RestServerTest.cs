using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using PostIt_Prototype_1.NetworkCommunicator;

namespace UnitTests
{
    [TestClass]
    public class RestServerTest
    {
        private RestServer _restServer;

        public RestServerTest()
        {
            _restServer = new RestServer();
        }
        [TestMethod]
        public async Task UpdateTest()
        {
            var query = new JObject { ["sessionID"] = "testSession" };
            var updates = new JObject { ["$addToSet"] = new JObject() { ["participants"] = "a@b.com" } };
            var json = new JObject
            {
                ["query"] = query,
                ["updates"] = updates
            };
            Assert.IsTrue(await _restServer.Update(json));
        }
        [TestMethod]
        public async Task InsertTest()
        {
            var json = new JObject
            {
                ["sessionID"] = "testSession",
                ["owner"] = "testOwner",
                ["participants"] = new JArray()
            };
            Assert.IsTrue(await _restServer.Insert(json));
        }
        [TestMethod]
        public async Task DeleteTest()
        {
            Assert.IsTrue(await _restServer.Delete(new Dictionary<string, object>
            {
                {"sessionID", "testSession"},
                {"owner", "testOwner"}
            }));
        }
        [TestMethod]
        public async Task QueryTest()
        {
            await _restServer.Query(new Dictionary<string, object>
            {
                {"sessionID", "testSession"},
                {"owner", "testOwner"}
            });
        }
    }
}
