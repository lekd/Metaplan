using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class WifiDataReceiver
    {
        public delegate void WifiDataReceivedEvent(byte[] receivedData);
        Socket _commsocket = null;

        public Socket Commsocket
        {
            get { return _commsocket; }
            set { _commsocket = value; }
        }
        public event WifiDataReceivedEvent _dataReceivedEventHandler = null;
        public WifiDataReceiver(Socket sock)
        {
            _commsocket = sock;
        }
        const int maxReceivedBytes = 25600;
        byte[] buffer = new byte[maxReceivedBytes];
        
        public void listenAndReceive()
        {
            
            _commsocket.BeginReceive(buffer, 0, maxReceivedBytes, SocketFlags.None,new AsyncCallback(Received),_commsocket);
        }
        private void Received(IAsyncResult result)
        {
            Socket socket = result.AsyncState as Socket;
            if (!socket.Connected)
            {
                return;
            }
            try
            {
                int receivedBytes = socket.EndReceive(result);
                if (receivedBytes > 0)
                {
                    if (_dataReceivedEventHandler != null)
                    {
                        byte[] actualReceivedData = new byte[receivedBytes];
                        Array.Copy(buffer, actualReceivedData, receivedBytes);
                        _dataReceivedEventHandler(actualReceivedData);
                    }
                }
                _commsocket.BeginReceive(buffer, 0, maxReceivedBytes, SocketFlags.None, new AsyncCallback(Received), _commsocket);
            }
            catch(Exception)
            {

            }
           
            
        }
        public void stop()
        {
            _commsocket.Close();
        }
    }
}
