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

namespace PostIt_Prototype_1.PostItObjects
{
    public enum PostItContentDataType
    {
        Text, Photo, WritingImage, NonDefined
    };
    public class PostItNote:IdeationUnit
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
                return  Convert.ToBase64String(Utilities.UtilitiesLib.BitmapToBytes((Image)content));
            }
            if (content is string)
            {
                return (string)content;
            }
            return null;
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
                Bitmap bmp = null;
                using (var ms = new MemoryStream(dataBytes))
                {
                    bmp = new Bitmap(ms);
                }
                if (dataType == PostItContentDataType.WritingImage)
                {
                    bmp.MakeTransparent(System.Drawing.Color.White);
                }
                //BitmapImage image = Utilities.UtilitiesLib.convertBitmapToBitmapImage(bmp);
                _content =  bmp;
            }
        }
    }
}
