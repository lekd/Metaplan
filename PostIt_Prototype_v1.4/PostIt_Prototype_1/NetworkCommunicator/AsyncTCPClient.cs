using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public class AsyncTCPClient
    {
        bool still_working;
        Socket workSocket = null;

        // ManualResetEvent instances signal completion.
        private ManualResetEvent connectDone =
            new ManualResetEvent(false);
        private ManualResetEvent sendDone =
            new ManualResetEvent(false);
        private ManualResetEvent receiveDone =
            new ManualResetEvent(false);

        private P2PClientListener clientDataListener = null;
        public void setP2PDataListener(P2PClientListener listener)
        {
            clientDataListener = listener;
        }
        public void StartClient()
        {
            var ipAddress = IPAddress.Parse(Properties.Settings.Default.IntermediateServerIP);
            var remoteEP = new IPEndPoint(ipAddress, Properties.Settings.Default.Port2IntermediateServer);

            // Create a TCP/IP socket.
            var client = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp);

            // Connect to the remote endpoint.
            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);
            connectDone.WaitOne();

            still_working = true;
            Receive(client);
        }
        public void Stop()
        {
            still_working = false;
        }
        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                var client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                workSocket = client;
                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                
            }
        }
        void Receive(Socket client)
        {
            if (!still_working)
            {
                client.Close();
                return;
            }
            try
            {
                // Create the state object.
                var state = new StateObject();
                state.workSocket = client;

                // Begin receiving the data from the remote device.
                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                    new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                
            }
        }
        void ReceiveCallback(IAsyncResult ar)
        {
            var state = (StateObject)ar.AsyncState;
            var handler = state.workSocket;

            var bytesRead = handler.EndReceive(ar);
            var content = String.Empty;
            if (bytesRead >0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.UTF8.GetString(
                    state.buffer, 0, bytesRead));

                
                if (clientDataListener != null)
                {
                    clientDataListener.P2PClientDataReceived(state.buffer,bytesRead);
                }
                Receive(handler);
            }
        }
        bool isSending = false;
        public void AsyncSend(byte[] data)
        {
            if (workSocket != null && !isSending)
            {
                workSocket.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), workSocket);
                isSending = true;
            }
        }
        public void SyncSend(byte[] data)
        {
            if (workSocket != null && !isSending)
            {
                var sendingThread = new Thread(() => SendAll(data));
                sendingThread.Start();
            }

        }
        void SendAll(byte[] data)
        {
            isSending = true;
            byte[] buffer = null;
            var MAX_BYTES_SENT = 1024;
            var index = 0;
            while (index < data.Length)
            {
                var remainingBytes = data.Length - index;
                var allocatedSize = MAX_BYTES_SENT;
                if (remainingBytes < MAX_BYTES_SENT)
                {
                    allocatedSize = remainingBytes;
                }
                buffer = new byte[allocatedSize];
                Array.Copy(data, index, buffer, 0, allocatedSize);
                workSocket.Send(buffer);
                index += allocatedSize;
            }
            isSending = false;
        }
        void SendCallback(IAsyncResult ar)
        {
            isSending = false;
        }
        static public byte[] createPacketHeader(byte[] packet)
        {
            var prefix = new byte[]{(byte)'@',(byte)':'};
            var postfix = new byte[] { (byte)':',(byte)'@'};
            var header_data = BitConverter.GetBytes(packet.Length);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(header_data);
            }
            var header = new byte[prefix.Length + header_data.Length + postfix.Length + 1];
            var index = 0;
            Array.Copy(prefix, 0, header, index, prefix.Length);
            index += prefix.Length;
            Array.Copy(header_data, 0, header, index, header_data.Length);
            index += header_data.Length;
            Array.Copy(postfix, 0, header, index, postfix.Length);
            index += postfix.Length;
            header[index] = 0;
            return header;
        }
    }
    public interface P2PClientListener
    {
        void P2PClientDataReceived(byte[] data, int receiveBytesNum);
    }
    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 13;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder sb = new StringBuilder();
    }
}
