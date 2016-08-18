using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            await _session.UpdateNotes();
        }

        [TestMethod()]
        public async Task UploadScreenShotAsyncTest()
        {
            await _session.UploadScreenShotAsync(new MemoryStream());
        }
        [TestMethod()]
        public async Task UploadNoteTest()
        {
            using (var m = File.OpenRead(@"D:\Users\alavis\Downloads\CELTIC_Notes\CELTIC_Notes\688565.png"))
            {
                await _session.UploadNoteAsync(m);
            }
                
        }
    }
}