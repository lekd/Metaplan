using System.Diagnostics;
using System.Linq;
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
            foreach (var participant in participants)
            {
                Trace.WriteLine(participant);
            } 
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

        [TestMethod()]
        public async Task CreateUserTest()
        {
            var session = new Session(Constants.SessionName, Constants.SessionOwnerID);
            var pm = new ParticipantManager(session, new RestServer());
            Assert.IsTrue(await pm.CreateUser(Constants.SessionOwnerID));
        }

        [TestMethod()]
        public async Task SignInTest()
        {
            Trace.WriteLine(await ParticipantManager.SignIn());
        }
    }
}