using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PostIt_Prototype_1.NetworkCommunicator;

namespace UnitTests
{
    [TestClass]
    public class SessionTest
    { 

        [TestMethod]
        public void TestNewSession()
        {
            var session = new Session("Session 1", "Ali");
            Assert.IsNotNull(session);
        }        
    }
}
