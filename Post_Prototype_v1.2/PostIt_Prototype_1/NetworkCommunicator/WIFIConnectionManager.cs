using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class WIFIConnectionManager
    {
        public delegate void DataReceivedEvent(byte[] receivedData);
        public delegate void ClientConnectedEvent(WifiDataReceiver receiver);
        Socket listeningSocket = null;
        public event ClientConnectedEvent clientConnectedHandler = null;
        public event DataReceivedEvent dataReceivedHandler = null;
        List<WifiDataReceiver> activeReceivers;
        public WIFIConnectionManager()
        {
            listeningSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            activeReceivers = new List<WifiDataReceiver>();
        }
        public void start()
        {
            listeningSocket.Bind(new IPEndPoint(0, 2015));
            listeningSocket.Listen(25);
            listeningSocket.BeginAccept(Accepted, listeningSocket);
        }
        private void Accepted(IAsyncResult result)
        {
            Socket listeningSock = result.AsyncState as Socket;
            Socket client = listeningSock.EndAccept(result);
            listeningSock.BeginAccept(Accepted, listeningSock);
            
            if (client != null)
            {
                WifiDataReceiver receiver = new WifiDataReceiver(client);
                activeReceivers.Add(receiver);
                receiver._dataReceivedEventHandler += receiver__dataReceivedEventHandler;
                Thread newThread = new Thread(new ThreadStart(receiver.listenAndReceive));
                newThread.SetApartmentState(ApartmentState.STA);
                newThread.Start();
                if (clientConnectedHandler != null)
                {
                    clientConnectedHandler(receiver);
                }
            }
        }

        void receiver__dataReceivedEventHandler(byte[] receivedData)
        {
            if (dataReceivedHandler != null)
            {
                dataReceivedHandler(receivedData);
            }
        }
        public void stop()
        {
            if (listeningSocket != null)
            {
                listeningSocket.Close();
            }
        }
    }
}
