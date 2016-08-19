using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using WhiteboardApp.NetworkCommunicator;

namespace UnitTests.NetworkCommunicator
{
    [TestClass()]
    public class SessionTests
    {
        private Session _session;

        public SessionTests()
        {
            _session = new Session(Constants.SessionName, Constants.SessionOwnerID);

        }
        [TestMethod()]
        public void ToStringTest()
        {
            var s = _session.ToString();
        }

        [TestMethod()]
        public async Task GetSessionNamesTest()
        {
            var a = await Session.GetSessionNames(Constants.SessionOwnerID);
        }

        [TestMethod()]
        public void CloseTest()
        {

        }

        [TestMethod()]
        public async Task CreateSessionAsyncTest()
        {
            await _session.CreateSessionAsync();
            /*var sessions = await Session.GetSessionNames(Constants.SessionOwnerID);
            var enumerable = sessions as string[] ?? sessions.ToArray();
            Assert.AreEqual(enumerable.Count(), 1);
            Assert.AreEqual(enumerable.First(), Constants.SessionName);
            await _session.DeleteSessionAsync();
            Assert.AreEqual(enumerable.Count(), 0);    */
        }

        [TestMethod()]
        public async Task GetSessionAsyncTest()
        {
            var s = await _session.GetSessionAsync();

            Trace.WriteLine(s);
            foreach (var e in s)
                Trace.WriteLine(e);
        }

        [TestMethod()]
        public async Task UpdateNotesTest()
        {
            var __session = new Session("Session A", "mercoproject@iwf.mavt.ethz.ch");

            await __session.UpdateNotes(new List<int>());
        }

        [TestMethod()]
        public async Task UploadScreenShotAsyncTest()
        {
            await _session.UploadScreenShotAsync(new MemoryStream());
        }
        [TestMethod()]
        public async Task GetScreenShotAsyncTest()
        {
            var __session = new Session("Session A", "mercoproject@iwf.mavt.ethz.ch");
            var s = await __session.GetSessionAsync();
            foreach (var e in s)
                Trace.WriteLine(e);
            //await __session.UploadScreenShotAsync(new MemoryStream());
        }
    }
}