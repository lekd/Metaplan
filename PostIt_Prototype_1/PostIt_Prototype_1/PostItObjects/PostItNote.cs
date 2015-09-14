using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenericIdeationObjects;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

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
        }
        public PostItContentDataType DataType
        {
            get { return _dataType; }
            set { _dataType = value; }
        }
        public static object ParseContentFromBytes(PostItContentDataType dataType, byte[] dataBytes)
        {
            if (dataType == PostItContentDataType.Text)
            {
                return Encoding.UTF8.GetString(dataBytes);
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
                return bmp;
            }
            return dataBytes;
        }
    }
}
