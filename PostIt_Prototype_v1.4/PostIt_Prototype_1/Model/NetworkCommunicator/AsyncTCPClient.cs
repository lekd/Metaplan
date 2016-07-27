using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class AsyncTcpClient
    {
        bool _stillWorking;
        Socket _workSocket = null;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent _connectDone =
            new ManualResetEvent(false);
        private ManualResetEvent _sendDone =
            new ManualResetEvent(false);
        private ManualResetEvent _receiveDone =
            new ManualResetEvent(false);

        private IP2PClientListener _clientDataListener = null;
        public void SetP2PDataListener(IP2PClientListener listener)
        {
            _clientDataListener = listener;
        }
        public void StartClient()
        {
            var ipAddress = IPAddress.Parse(Properties.Settings.Default.IntermediateServerIP);
            var remoteEp = new IPEndPoint(ipAddress, Properties.Settings.Default.Port2IntermediateServer);

            // Create a TCP/IP socket.
            var client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            client.BeginConnect(remoteEp,
                new AsyncCallback(ConnectCallback), client);
            _connectDone.WaitOne();

            _stillWorking = true;
            Receive(client);
        }
        public void Stop()
        {
            _stillWorking = false;
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                _workSocket = client;
                // Signal that the connection has been made.
                _connectDone.Set();
            }
            catch (Exception e)
            {
                Utilities.UtilitiesLib.LogError(e);
            }
        }
        void Receive(Socket client)
        {
            if (!_stillWorking)
            {
                client.Close();
                return;
            }
            try
            {
                // Create the state object.
                var state = new StateObject();
                state.WorkSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.Buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                Utilities.UtilitiesLib.LogError(e);
            }
        }
        void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var handler = state.WorkSocket;

            var bytesRead = handler.EndReceive(ar);
            var content = String.Empty;
            if (bytesRead >0)
            {
                // There  might be more data, so store the data received so far.
                state.Sb.Append(Encoding.UTF8.GetString(
                    state.Buffer, 0, bytesRead));

                
                if (_clientDataListener != null)
                {
                    _clientDataListener.P2PClientDataReceived(state.Buffer,bytesRead);
                }
                Receive(handler);
            }
        }
        bool _isSending = false;
        public void AsyncSend(byte[] data)
        {
            if (_workSocket != null && !_isSending)
            {
                _workSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), _workSocket);
                _isSending = true;
            }
        }
        public void SyncSend(byte[] data)
        {
            if (_workSocket != null && !_isSending)
            {
                var sendingThread = new Thread(() => SendAll(data));
                sendingThread.Start();
            }

        }
        void SendAll(byte[] data)
        {
            _isSending = true;
            byte[] buffer = null;
            var maxBytesSent = 1024;
            var index = 0;
            while (index < data.Length)
            {
                var remainingBytes = data.Length - index;
                var allocatedSize = maxBytesSent;
                if (remainingBytes < maxBytesSent)
                {
                    allocatedSize = remainingBytes;
                }
                buffer = new byte[allocatedSize];
                Array.Copy(data, index, buffer, 0, allocatedSize);
                _workSocket.Send(buffer);
                index += allocatedSize;
            }
            _isSending = false;
        }
        void SendCallback(IAsyncResult ar)
        {
            _isSending = false;
        }
        static public byte[] CreatePacketHeader(byte[] packet)
        {
            var prefix = new byte[]{(byte)'@',(byte)':'};
            var postfix = new byte[] { (byte)':',(byte)'@'};
            var headerData = BitConverter.GetBytes(packet.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(headerData);
            }
            var header = new byte[prefix.Length + headerData.Length + postfix.Length + 1];
            var index = 0;
            Array.Copy(prefix, 0, header, index, prefix.Length);
            index += prefix.Length;
            Array.Copy(headerData, 0, header, index, headerData.Length);
            index += headerData.Length;
            Array.Copy(postfix, 0, header, index, postfix.Length);
            index += postfix.Length;
            header[index] = 0;
            return header;
        }
    }
    public interface IP2PClientListener
    {
        void P2PClientDataReceived(byte[] data, int receiveBytesNum);
    }
    public class StateObject
    {
        // Client socket.
        public Socket WorkSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 13;
        // Receive buffer.
        public byte[] Buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder Sb = new StringBuilder();
    }
}
