using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostIt_Prototype_1.PostItObjects;
using PostIt_Prototype_1.NetworkCommunicator;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class RemotePointerManager:IP2PClientListener
    {
        string[] _pointerColors = new string[] { "#ff0000", "#00ff00", "#0000ff", "#800080", "#00ffff", "#ff6600" };
        int _nextColorIndex = 0;
        Dictionary<int, RemotePointer> _remotePointerList;
        IPointerManagerEventListener _pointerEventListener = null;
        public void SetPointerEventListener(IPointerManagerEventListener listener)
        {
            _pointerEventListener = listener;
        }
        public RemotePointerManager()
        {
            _remotePointerList = new Dictionary<int, RemotePointer>();
        }
        public void P2PClientDataReceived(byte[] data, int receiveBytesNum)
        {
            if (receiveBytesNum < RemotePointer.PackageLength)
            {
                return;
            }
            var sb = new StringBuilder();
            sb.Append(Encoding.UTF8.GetString(data, 0, receiveBytesNum));
            var dataStr = sb.ToString();
            if (dataStr.IndexOf("\n") < RemotePointer.PackageLength - 1)
            {
                return;
            }
            var validDataChunk = new byte[RemotePointer.PackageLength];
            Array.Copy(data,dataStr.IndexOf("\n") - (RemotePointer.PackageLength - 1), validDataChunk, 0, RemotePointer.PackageLength);
            var remotePointer = new RemotePointer();
            remotePointer.Parse(validDataChunk);
            if (!_remotePointerList.ContainsKey(remotePointer.Id))
            {
                if (remotePointer.X >= 0 && remotePointer.X <= 1
                    && remotePointer.Y >= 0 && remotePointer.Y <= 1)
                {
                    remotePointer.IsActive = true;
                    _remotePointerList.Add(remotePointer.Id, remotePointer);
                    if (_pointerEventListener != null)
                    {
                        _pointerEventListener.NewPointerAddedEvent(remotePointer,_pointerColors[_nextColorIndex]);
                        _nextColorIndex = (_nextColorIndex + 1)%_pointerColors.Length;
                    }
                }
            }
            else
            {
                if (remotePointer.X >= 0 && remotePointer.X <= 1
                    && remotePointer.Y >= 0 && remotePointer.Y <= 1)
                {
                    remotePointer.IsActive = true;
                    _remotePointerList[remotePointer.Id] = remotePointer;
                    if (_pointerEventListener != null)
                    {
                        _pointerEventListener.PointerUpdatedEvent(remotePointer);
                    }
                }
                else
                {
                    remotePointer.IsActive = false;
                    _remotePointerList[remotePointer.Id] = remotePointer;
                    if (_pointerEventListener != null)
                    {
                        _pointerEventListener.PointerUpdatedEvent(remotePointer);
                    }
                }
            }
        }
    }
    public interface IPointerManagerEventListener
    {
        void NewPointerAddedEvent(RemotePointer addedPointer, string assignedColorCode);
        void PointerUpdatedEvent(RemotePointer updatedPointer);
    }
}
