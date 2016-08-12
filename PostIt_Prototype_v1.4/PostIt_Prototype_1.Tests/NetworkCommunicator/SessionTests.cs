using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhiteboardApp.NetworkCommunicator;

namespace WhiteboardApp.Tests.NetworkCommunicator
{
    [TestClass()]
    public class SessionTests
    {
        private Session _session;

        public SessionTests()
        {
            _session = new Session(Constants.SessionName, Constants.SessionOwner);

        }
        [TestMethod()]
        public void ToStringTest()
        {
            var s = _session.ToString();
        }

        [TestMethod()]
        public async Task GetSessionNamesTest()
        {
            var a = await Session.GetSessionNames(Constants.SessionOwner);
        }

        [TestMethod()]
        public void CloseTest()
        {
           
        }

        [TestMethod()]
        public async Task CreateSessionAsyncTest()
        {
            await _session.CreateSessionAsync();
            await _session.DeleteSessionAsync();

        }

        [TestMethod()]
        public async Task GetSessionAsyncTest()
        {
            await _session.GetSessionAsync();
        }

        [TestMethod()]
        public void UpdateNotesTest()
        {
            throw new NotImplementedException();
        }
    }
}