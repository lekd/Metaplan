using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WhiteboardApp.NetworkCommunicator;

namespace UnitTests.NetworkCommunicator
{
    [TestClass()]
    public class ParticipantManagerTests
    {
        [TestMethod]
        public async Task GetParticipantsTest()
        {
            var session = new Session(Constants.SessionName, Constants.SessionOwnerID);
            var pm = new ParticipantManager(session, new RestServer());
            var participants = await pm.GetParticipants();
        }

        [TestMethod()]
        public void ParticipantManagerTest()
        {
            var session = new Session(Constants.SessionName, Constants.SessionOwnerID);
            var pm = new ParticipantManager(session, new RestServer());
            Assert.IsNotNull(pm);
        }

        [TestMethod()]
        public async Task AddParticipantTest()
        {
            var session = new Session(Constants.SessionName, Constants.SessionOwnerID);
            var pm = new ParticipantManager(session, new RestServer());
            Assert.IsTrue(await pm.AddParticipant(Constants.Email));
        }

    }
}