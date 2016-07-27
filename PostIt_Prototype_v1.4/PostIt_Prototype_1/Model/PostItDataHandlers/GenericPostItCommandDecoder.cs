using System;
using System.Text;
using PostIt_Prototype_1.ModelView.PostItObjects;
using PostIt_Prototype_1.PostItDataHandlers;

namespace PostIt_Prototype_1.Model.PostItDataHandlers
{
    public enum PostItCommandType
    {
        Add, Update, Delete, NotDefined
    };
    public class GenericPostItCommandDecoder:IPostItNetworkDataHandler
    {
        byte[] _buffer = null;
        public delegate void PostItCommandDecodedEvent(PostItCommandType command, object arg);
        public event PostItCommandDecodedEvent CommandDecodedEventHandler = null;

        private byte[] ConcatWithRemainingBuffer(byte[] newData)
        {
            var oldDataLength = 0;
            if (_buffer != null)
            {
                oldDataLength = _buffer.Length;
            }
            var allData = new byte[oldDataLength + newData.Length];
            var index = 0;
            if (_buffer != null)
            {
                Array.Copy(_buffer, allData, _buffer.Length);
                index += _buffer.Length;
            }
            Array.Copy(newData, 0, allData, index, newData.Length);
            return allData;
        }
        public void DecodeCommandInByteArray(byte[] data)
        {
            var allData = ConcatWithRemainingBuffer(data);
            var commandType = ClassifyCommand(allData);
            PostItCommand command = null;
            switch (commandType)
            {
                case PostItCommandType.Add:
                    command = DecodeAddCommand(allData);
                    break;
                case PostItCommandType.Update:
                    command = DecodeUpdateCommand(allData);
                    break;
                case PostItCommandType.Delete:
                    command = DecodeDeleteCommand(allData);
                    break;
            }
            if (command != null)
            {
                CommandDecodedEventHandler?.Invoke(command.CommandType, command.CommandData);
                _buffer = null;
            }
            else
            {
                _buffer = allData;
            }
        }
        PostItCommand DecodeAddCommand(byte[] data)
        {
            var commndStr = Encoding.UTF8.GetString(data);
            if (!commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.AddPrefix))
                || !commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.AddPostfix)))
            {
                return null;
            }
            if(!commndStr.StartsWith(Encoding.UTF8.GetString(PostItCommand.AddPrefix)))
            {
                return null;
            }
            //start to extract part of the message;
            //structure of an Add command
            //<ADD> ID X Y Orientation Size DataType Data </ADD>
            var command = new PostItCommand {CommandType = PostItCommandType.Add};
            var index = PostItCommand.AddPrefix.Length;
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
        PostItCommand DecodeUpdateCommand(byte[] data)
        {
            var commndStr = Encoding.UTF8.GetString(data);
            if (!commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.UpdatePrefix))
                || !commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.UpdatePostfix)))
            {
                return null;
            }
            if (!commndStr.StartsWith(Encoding.UTF8.GetString(PostItCommand.UpdatePrefix)))
            {
                return null;
            }
            //start to extract part of the message;
            //structure of an Add command
            //<UPD> ID X Y Orientation </UPD>
            var command = new PostItCommand();
            command.CommandType = PostItCommandType.Update;
            var index = PostItCommand.UpdatePrefix.Length;
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
        PostItCommand DecodeDeleteCommand(byte[] data)
        {
            var commndStr = Encoding.UTF8.GetString(data);
            if (!commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.DelPrefix))
                || !commndStr.Contains(Encoding.UTF8.GetString(PostItCommand.DelPostfix)))
            {
                return null;
            }
            var correctMsgLength = PostItCommand.DelPrefix.Length + PostItCommand.DelPostfix.Length + sizeof(Int32);
            if (data.Length != correctMsgLength)
            {
                return null;
            }
            var command = new PostItCommand();
            command.CommandType = PostItCommandType.Delete;
            var cmdData = new byte[sizeof(Int32)];
            Array.Copy(data, PostItCommand.DelPrefix.Length, cmdData, 0, cmdData.Length);
            command.CommandData = Utilities.UtilitiesLib.Bytes2Int(cmdData);
            return command;
        }
        PostItCommandType ClassifyCommand(byte[] data)
        {
            var dataStr = Encoding.UTF8.GetString(data);
            if (dataStr.Contains(Encoding.UTF8.GetString(PostItCommand.AddPrefix)))
            {
                return PostItCommandType.Add;
            }
            if (dataStr.Contains(Encoding.UTF8.GetString(PostItCommand.UpdatePrefix)))
            {
                return PostItCommandType.Update;
            }
            if (dataStr.Contains(Encoding.UTF8.GetString(PostItCommand.DelPrefix)))
            {
                return PostItCommandType.Delete;
            }
            return PostItCommandType.NotDefined;
        }
    }
    class PostItCommand
    {
        static public byte[] AddPrefix = new byte[] {(byte)'<',(byte)'A',(byte)'D',(byte)'D',(byte)'>'};
        static public byte[] AddPostfix = new byte[] { (byte)'<',(byte)'/' , (byte)'A', (byte)'D', (byte)'D', (byte)'>' };
        static public byte[] UpdatePrefix = new byte[] { (byte)'<', (byte)'U', (byte)'P', (byte)'D', (byte)'>' };
        static public byte[] UpdatePostfix = new byte[] { (byte)'<', (byte)'/', (byte)'U', (byte)'P', (byte)'D', (byte)'>' };
        static public byte[] DelPrefix = new byte[] { (byte)'<', (byte)'D', (byte)'E', (byte)'L', (byte)'>' };
        static public byte[] DelPostfix = new byte[] { (byte)'<', (byte)'/', (byte)'D', (byte)'E', (byte)'L', (byte)'>' };
        static public byte[] TextType = new byte[] {(byte)'@',(byte)'T',(byte)'X',(byte)'T'};
        static public byte[] ImgType = new byte[] { (byte)'@', (byte)'I', (byte)'M', (byte)'G' };
        static public byte[] TransparentImageType = new byte[] { (byte)'@', (byte)'B', (byte)'M', (byte)'P' };
        
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
            if (str.Contains(Encoding.UTF8.GetString(TextType)))
            {
                return PostItContentDataType.Text;
            }
            if (str.Contains(Encoding.UTF8.GetString(ImgType)))
            {
                return PostItContentDataType.Photo;
            }
            if (str.Contains(Encoding.UTF8.GetString(TransparentImageType)))
            {
                return PostItContentDataType.WritingImage;
            }
            return PostItContentDataType.NonDefined;
        }
    }
}
