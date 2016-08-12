using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PostIt_Prototype_1.NetworkCommunicator;

namespace UnitTests
{
    [TestClass]
    public class ParticipantManagerTest
    {
        [TestMethod]
        public void TestNewParticipantManager()
        {
            var session = new Session("Session 1", "Ali");
            var pm = new ParticipantManager(session, new RestServer());
            Assert.IsNotNull(pm);
        }

        [TestMethod]
        public async Task TestGetParticipants()
        {
            var session = new Session("Session 1", "Ali");
            var pm = new ParticipantManager(session, new RestServer());
            var participants = await pm.GetParticipants();
        }

        [TestMethod]
        public async Task TestAddParticipant()
        {
            var session = new Session("Session 1", "Ali");
            var pm = new ParticipantManager(session, new RestServer());
            Assert.IsTrue(await pm.AddParticipant("ali@alavi.com"));
        }

    }
}
