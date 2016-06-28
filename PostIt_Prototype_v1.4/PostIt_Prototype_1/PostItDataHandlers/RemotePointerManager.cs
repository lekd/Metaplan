using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostIt_Prototype_1.PostItObjects;
using PostIt_Prototype_1.NetworkCommunicator;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class RemotePointerManager:P2PClientListener
    {
        string[] pointerColors = new string[] { "#ff0000", "#00ff00", "#0000ff", "#800080", "#00ffff", "#ff6600" };
        int nextColorIndex = 0;
        Dictionary<int, RemotePointer> remotePointerList;
        PointerManagerEventListener pointerEventListener = null;
        public void setPointerEventListener(PointerManagerEventListener listener)
        {
            pointerEventListener = listener;
        }
        public RemotePointerManager()
        {
            remotePointerList = new Dictionary<int, RemotePointer>();
        }
        public void P2PClientDataReceived(byte[] data, int receiveBytesNum)
        {
            if (receiveBytesNum < RemotePointer.PackageLength)
            {
                return;
            }
            StringBuilder sb = new StringBuilder();
            sb.Append(Encoding.UTF8.GetString(data, 0, receiveBytesNum));
            string dataStr = sb.ToString();
            if (dataStr.IndexOf("\n") < RemotePointer.PackageLength - 1)
            {
                return;
            }
            byte[] validDataChunk = new byte[RemotePointer.PackageLength];
            Array.Copy(data,dataStr.IndexOf("\n") - (RemotePointer.PackageLength - 1), validDataChunk, 0, RemotePointer.PackageLength);
            RemotePointer remotePointer = new RemotePointer();
            remotePointer.Parse(validDataChunk);
            if (!remotePointerList.ContainsKey(remotePointer.Id))
            {
                if (remotePointer.X >= 0 && remotePointer.X <= 1
                    && remotePointer.Y >= 0 && remotePointer.Y <= 1)
                {
                    remotePointer.IsActive = true;
                    remotePointerList.Add(remotePointer.Id, remotePointer);
                    if (pointerEventListener != null)
                    {
                        pointerEventListener.NewPointerAddedEvent(remotePointer,pointerColors[nextColorIndex]);
                        nextColorIndex = (nextColorIndex + 1)%pointerColors.Length;
                    }
                }
            }
            else
            {
                if (remotePointer.X >= 0 && remotePointer.X <= 1
                    && remotePointer.Y >= 0 && remotePointer.Y <= 1)
                {
                    remotePointer.IsActive = true;
                    remotePointerList[remotePointer.Id] = remotePointer;
                    if (pointerEventListener != null)
                    {
                        pointerEventListener.PointerUpdatedEvent(remotePointer);
                    }
                }
                else
                {
                    remotePointer.IsActive = false;
                    remotePointerList[remotePointer.Id] = remotePointer;
                    if (pointerEventListener != null)
                    {
                        pointerEventListener.PointerUpdatedEvent(remotePointer);
                    }
                }
            }
        }
    }
    public interface PointerManagerEventListener
    {
        void NewPointerAddedEvent(RemotePointer addedPointer, string assignedColorCode);
        void PointerUpdatedEvent(RemotePointer updatedPointer);
    }
}
