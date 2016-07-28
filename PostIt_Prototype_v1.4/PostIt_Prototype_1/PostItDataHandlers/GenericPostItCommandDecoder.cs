using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostIt_Prototype_1.PostItObjects;
using System.IO;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public enum PostItCommandType
    {
        Add, Update, Delete, NotDefined
    };
    public class GenericPostItCommandDecoder:IPostItNetworkDataHandler
    {
        byte[] buffer = null;
        public delegate void PostItCommandDecodedEvent(PostItCommandType command, object arg);
        public event PostItCommandDecodedEvent commandDecodedEventHandler = null;
        byte[] concatWithRemainingBuffer(byte[] newData)
        {
            var oldDataLength = 0;
            if (buffer != null)
            {
                oldDataLength = buffer.Length;
            }
            var allData = new byte[oldDataLength + newData.Length];
            var index = 0;
            if (buffer != null)
            {
                Array.Copy(buffer, allData, buffer.Length);
                index += buffer.Length;
            }
            Array.Copy(newData, 0, allData, index, newData.Length);
            return allData;
        }
        public void decodeCommandInByteArray(byte[] data)
        {
            var allData = concatWithRemainingBuffer(data);
            var commandType = classifyCommand(allData);
            PostItCommand command = null;
            switch (commandType)
            {
                case PostItCommandType.Add:
                    command = decodeAddCommand(allData);
                    break;
                case PostItCommandType.Update:
                    command = decodeUpdateCommand(allData);
                    break;
                case PostItCommandType.Delete:
                    command = decodeDeleteCommand(allData);
                    break;
            }
            if (command != null)
            {
                if (commandDecodedEventHandler != null)
                {
                    commandDecodedEventHandler(command.CommandType, command.CommandData);
                }
                buffer = null;
            }
            else
            {
                buffer = allData;
            }
        }
        PostItCommand decodeAddCommand(byte[] data)
        {
            var commndStr = Encoding.UTF8.GetString(data);
            if (!commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.ADD_PREFIX))
                || !commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.ADD_POSTFIX)))
            {
                return null;
            }
            if(!commndStr.StartsWith(Encoding.UTF8.GetString(PostItCommand.ADD_PREFIX)))
            {
                return null;
            }
            //start to extract part of the message;
            //structure of an Add command
            //<ADD> ID X Y Orientation Size DataType Data </ADD>
            var command = new PostItCommand();
            command.CommandType = PostItCommandType.Add;
            var index = PostItCommand.ADD_PREFIX.Length;
            var note = new PostItNote();
            //ID
            var buffer = new byte[4];
            Array.Copy(data, index, buffer, 0, 4);
            note.Id = Utilities.UtilitiesLib.Bytes2Int(buffer);
            index += 4;
            //X
            Array.Copy(data, index, buffer, 0, 4);
            note.CenterX = Utilities.UtilitiesLib.Bytes2Float(buffer);
            index += 4;
            //Y
            Array.Copy(data, index, buffer, 0, 4);
            note.CenterY = Utilities.UtilitiesLib.Bytes2Float(buffer);
            index += 4;
            //skip 4 bytes of orientation, can be used in the future
            index += 4;
            //get size of the content
            Array.Copy(data, index, buffer, 0, 4);
            var contentSize = Utilities.UtilitiesLib.Bytes2Int(buffer);
            index += 4;
            //get content data type
            Array.Copy(data, index, buffer, 0, 4);
            note.DataType = PostItCommand.GetPostItContentType(buffer);
            index += 4;
            //start getting content
            buffer = new byte[contentSize];
            Array.Copy(data, index, buffer, 0, buffer.Length);
            note.ParseContentFromBytes(note.DataType, buffer);
            command.CommandData = note;
            return command;
        }
        PostItCommand decodeUpdateCommand(byte[] data)
        {
            var commndStr = Encoding.UTF8.GetString(data);
            if (!commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.UPDATE_PREFIX))
                || !commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.UPDATE_POSTFIX)))
            {
                return null;
            }
            if (!commndStr.StartsWith(Encoding.UTF8.GetString(PostItCommand.UPDATE_PREFIX)))
            {
                return null;
            }
            //start to extract part of the message;
            //structure of an Add command
            //<UPD> ID X Y Orientation </UPD>
            var command = new PostItCommand();
            command.CommandType = PostItCommandType.Update;
            var index = PostItCommand.UPDATE_PREFIX.Length;
            var note = new PostItNote();
            //ID
            var buffer = new byte[4];
            Array.Copy(data, index, buffer, 0, 4);
            note.Id = Utilities.UtilitiesLib.Bytes2Int(buffer);
            index += 4;
            //X
            Array.Copy(data, index, buffer, 0, 4);
            note.CenterX = Utilities.UtilitiesLib.Bytes2Float(buffer);
            index += 4;
            //Y
            Array.Copy(data, index, buffer, 0, 4);
            note.CenterY = Utilities.UtilitiesLib.Bytes2Float(buffer);
            //at this moment, do not read the Orientation
            command.CommandData = note;
            return command;
        }
        PostItCommand decodeDeleteCommand(byte[] data)
        {
            var commndStr = Encoding.UTF8.GetString(data);
            if (!commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.DEL_PREFIX))
                || !commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.DEL_POSTFIX)))
            {
                return null;
            }
            var correctMsgLength = PostItCommand.DEL_PREFIX.Length + PostItCommand.DEL_POSTFIX.Length + sizeof(Int32);
            if (data.Length != correctMsgLength)
            {
                return null;
            }
            var command = new PostItCommand();
            command.CommandType = PostItCommandType.Delete;
            var cmdData = new byte[sizeof(Int32)];
            Array.Copy(data, PostItCommand.DEL_PREFIX.Length, cmdData, 0, cmdData.Length);
            command.CommandData = Utilities.UtilitiesLib.Bytes2Int(cmdData);
            return command;
        }
        PostItCommandType classifyCommand(byte[] data)
        {
            var dataStr = Encoding.UTF8.GetString(data);
            if (dataStr.Contains(Encoding.UTF8.GetString(PostItCommand.ADD_PREFIX)))
            {
                return PostItCommandType.Add;
            }
            if (dataStr.Contains(Encoding.UTF8.GetString(PostItCommand.UPDATE_PREFIX)))
            {
                return PostItCommandType.Update;
            }
            if (dataStr.Contains(Encoding.UTF8.GetString(PostItCommand.DEL_PREFIX)))
            {
                return PostItCommandType.Delete;
            }
            return PostItCommandType.NotDefined;
        }
    }
    class PostItCommand
    {
        static public byte[] ADD_PREFIX = new byte[] {(byte)'<',(byte)'A',(byte)'D',(byte)'D',(byte)'>'};
        static public byte[] ADD_POSTFIX = new byte[] { (byte)'<',(byte)'/' , (byte)'A', (byte)'D', (byte)'D', (byte)'>' };
        static public byte[] UPDATE_PREFIX = new byte[] { (byte)'<', (byte)'U', (byte)'P', (byte)'D', (byte)'>' };
        static public byte[] UPDATE_POSTFIX = new byte[] { (byte)'<', (byte)'/', (byte)'U', (byte)'P', (byte)'D', (byte)'>' };
        static public byte[] DEL_PREFIX = new byte[] { (byte)'<', (byte)'D', (byte)'E', (byte)'L', (byte)'>' };
        static public byte[] DEL_POSTFIX = new byte[] { (byte)'<', (byte)'/', (byte)'D', (byte)'E', (byte)'L', (byte)'>' };
        static public byte[] TEXT_TYPE = new byte[] {(byte)'@',(byte)'T',(byte)'X',(byte)'T'};
        static public byte[] IMG_TYPE = new byte[] { (byte)'@', (byte)'I', (byte)'M', (byte)'G' };
        static public byte[] TRANSPARENT_IMAGE_TYPE = new byte[] { (byte)'@', (byte)'B', (byte)'M', (byte)'P' };
        
        PostItCommandType _commandType;

        public PostItCommandType CommandType
        {
            get { return _commandType; }
            set { _commandType = value; }
        }
        object _commandData;

        public object CommandData
        {
            get { return _commandData; }
            set { _commandData = value; }
        }

        static public PostItContentDataType GetPostItContentType(byte[] typeBytes)
        {
            var str = Encoding.UTF8.GetString(typeBytes);
            if (str.Contains(Encoding.UTF8.GetString(TEXT_TYPE)))
            {
                return PostItContentDataType.Text;
            }
            if (str.Contains(Encoding.UTF8.GetString(IMG_TYPE)))
            {
                return PostItContentDataType.Photo;
            }
            if (str.Contains(Encoding.UTF8.GetString(TRANSPARENT_IMAGE_TYPE)))
            {
                return PostItContentDataType.WritingImage;
            }
            return PostItContentDataType.NonDefined;
        }
    }
}
