using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using WhiteboardApp.NetworkCommunicator;
using WhiteboardApp.Utilities;

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
            Assert.IsTrue(await _restServer.Delete(Session.Collection, new Dictionary<string, object>
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
                {"sessionID", "Session A"},
                {"owner", "mercoproject@iwf.mavt.ethz.ch"},
                {"participants" , "alialavia@gmail.com" }
            });
        }

        [TestMethod]
        public async Task TestGetAllData()
        {
            await _restServer.Query(Session.Collection, new Dictionary<string, object>());
        }

        [TestMethod]
        public async Task TestGetDirectoryList()
        {
            Trace.WriteLine("Get dir");
            var r =
                await
                    _restServer.Query("files",
                        new Dictionary<string, object> {{"path", "sessions.TestOwnerID.Test Session.notes"}});
            /*var buffer = r[1]["Content"]["data"];
             */
            Assert.IsNull(r);
            //Assert.AreEqual(r.Count, 0);
            //File.WriteAllBytes(@"D:\test.png", (from b in buffer select (byte)b).ToArray());
        }
        /*
        [TestMethod]
        public async Task TestRepetetiveGetDirectoryList()
        {            
            var fileNumbers = -1;
            for (int i = 0; i < 10; i++)
            {
                Trace.WriteLine("Get dir");
                var r =
                    await
                        _restServer.Query("files",
                            new Dictionary<string, object> {{"path", "sessions.TestOwnerIDundefined"}});                
                
                if (fileNumbers == -1)
                    fileNumbers = r.Count;
                Trace.WriteLine($"{r.Count} ?= {fileNumbers}");
                Assert.AreEqual(fileNumbers, r.Count);
            }            
        }
        */
    }
}
