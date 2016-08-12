using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteboardApp.PostItDataHandlers
{
    public class AnotoInkDot
    {
        private int _paperNoteID;

        public int PaperNoteID
        {
            get { return _paperNoteID; }
            set { _paperNoteID = value; }
        }
        private float _x;

        public float X
        {
            get { return _x; }
            set { _x = value; }
        }
        private float _y;

        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }
        public AnotoInkDot(int paperID = 0,float x=0,float y=0)
        {
            _paperNoteID = paperID;
            _x = 0;
            _y = 0;
        }
        public void parseFromRawBytes(byte[] data)
        {
            var buffer = new byte[4];
            Array.Copy(data, 0, buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            _paperNoteID = BitConverter.ToInt32(buffer, 0);
            Array.Copy(data, 4, buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            _x = BitConverter.ToSingle(buffer, 0);
            Array.Copy(data, 8, buffer, 0, 4);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(buffer);
            }
            _y = BitConverter.ToSingle(buffer, 0);
        }
        public static int size()
        {
            return (sizeof(int) + 2 * sizeof(float));
        }
    }
}
