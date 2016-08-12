﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteboardApp.PostItObjects
{
    public class RemotePointer
    {
        public const int PackageLength = 13;
        int _id;

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }
        float _x;

        public float X
        {
            get { return _x; }
            set { _x = value; }
        }
        float _y;

        public float Y
        {
            get { return _y; }
            set { _y = value; }
        }
        bool _isActive;

        public bool IsActive
        {
            get { return _isActive; }
            set { _isActive = value; }
        }
        public RemotePointer()
        {
            _isActive = true;
        }
        public void Parse(byte[] InBytes)
        {
            var buffer = new byte[4];
            var index = 0;
            Array.Copy(InBytes, index, buffer, 0, 4);
            Array.Reverse(buffer);
            _id = BitConverter.ToInt32(buffer, 0);
            index += 4;

            Array.Copy(InBytes, index, buffer, 0, 4);
            Array.Reverse(buffer);
            _x = BitConverter.ToSingle(buffer, 0);
            index += 4;

            Array.Copy(InBytes, index, buffer, 0, 4);
            Array.Reverse(buffer);
            _y = BitConverter.ToSingle(buffer, 0);
        }
        public string toString()
        {
            return $"{_id}:{_x}-{_y}";
        }
    }
}
