using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using GenericIdeationObjects;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.ModelView.PostItObjects
{
    public enum PostItContentDataType
    {
        Text, Photo, WritingImage, NonDefined
    };
    public class PostItNote : IdeationUnit
    {
        PostItContentDataType _dataType = PostItContentDataType.NonDefined;
        PostItMetaData _metaData;

        public PostItMetaData MetaData
        {
            get { return _metaData; }
            set { _metaData = value; }
        }
        public PostItNote()
        {

            _content = null;
            _isAvailable = true;
            _metaData = new PostItMetaData();
        }
        public PostItContentDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }
        public static string GetDatatStringOfIdeaContent(object content)
        {
            if (content is Bitmap)
            {
                return Convert.ToBase64String(Utilities.UtilitiesLib.BitmapToBytes((Image)content));
            }
            if (content is string)
            {
                return (string)content;
            }
            return null;
        }
        public override IdeationUnit Clone()
        {
            var clonedNote = new PostItNote();
            clonedNote.Id = _id;
            clonedNote.CenterX = _centerX;
            clonedNote.CenterY = _centerY;
            clonedNote.IsAvailable = _isAvailable;
            clonedNote.DataType = _dataType;
            if (_content is Bitmap)
            {
                clonedNote.Content = ((Bitmap)_content).Clone();
            }
            return clonedNote;
        }
        public void ParseContentFromBytes(PostItContentDataType dataType, byte[] dataBytes)
        {
            if (dataType == PostItContentDataType.Text)
            {
                _content = Encoding.UTF8.GetString(dataBytes);
            }
            if (dataType == PostItContentDataType.Photo
                || dataType == PostItContentDataType.WritingImage)
            {
                var img = (new ImageConverter()).ConvertFrom(dataBytes) as Image;
                var bmp = new Bitmap(img);

                if (dataType == PostItContentDataType.WritingImage)
                {
                    bmp.MakeTransparent(System.Drawing.Color.White);
                }
                //BitmapImage image = Utilities.UtilitiesLib.convertBitmapToBitmapImage(bmp);

                _content = bmp;
            }

        }
        public static T DeepClone<T>(T a)
        {
            using (var stream = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
