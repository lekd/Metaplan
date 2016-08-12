using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WhiteboardApp.NetworkCommunicator;

namespace UnitTests.NetworkCommunicator
{
    [TestClass]
    public class RestServerTests
    {
        private readonly RestServer _restServer;

        public RestServerTests()
        {
            _restServer = new RestServer();
        }
        [TestMethod]
        public async Task UpdateTest()
        {
            var query = new JObject { ["sessionID"] = Constants.SessionName };
            var updates = new JObject { ["$addToSet"] = new JObject() { ["participants"] = Constants.Email } };
            var json = new JObject
            {
                ["query"] = query,
                ["updates"] = updates
            };
            Assert.IsTrue(await _restServer.Update(Session.Collection, json));
        }
        [TestMethod]
        public async Task InsertTest()
        {
            var json = new JObject
            {
                ["sessionID"] = Constants.SessionName,
                ["owner"] = Constants.SessionOwnerID,
                ["participants"] = new JArray()
            };
            Assert.IsTrue(await _restServer.Insert(Session.Collection, json));
        }
        [TestMethod]
        public async Task DeleteTest()
        {
            Assert.IsTrue(await _restServer.Delete(Session.Collection,new Dictionary <string, object>
            {
                {"sessionID", Constants.SessionName},
                {"owner", Constants.SessionOwnerID}
            }));
        }
        [TestMethod]
        public async Task QueryTest()
        {
            await _restServer.Query(Session.Collection, new Dictionary<string, object>
            {
                {"sessionID", Constants.SessionName},
                {"owner", Constants.SessionOwnerID}
            });
        }

        [TestMethod]
        public async Task TestGetAllData()
        {
            await _restServer.Query(Session.Collection, new Dictionary<string, object>());
        }
    }
}
