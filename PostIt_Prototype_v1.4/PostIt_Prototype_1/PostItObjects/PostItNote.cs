using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenericIdeationObjects;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Runtime.Serialization.Formatters.Binary;

namespace PostIt_Prototype_1.PostItObjects
{
    public enum PostItContentDataType
    {
        Text, Photo, WritingImage, NonDefined
    };
    public class PostItNote : IdeationUnit
    {
        PostItContentDataType _dataType = PostItContentDataType.NonDefined;
        public PostItNote()
        {

            _content = null;
            _isAvailable = true;
        }
        public PostItContentDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }
        public static string getDatatStringOfIdeaContent(object content)
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
            PostItNote clonedNote = new PostItNote();
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
                Image img = (new ImageConverter()).ConvertFrom(dataBytes) as Image;
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
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, a);
                stream.Position = 0;
                return (T)formatter.Deserialize(stream);
            }
        }
    }
}
