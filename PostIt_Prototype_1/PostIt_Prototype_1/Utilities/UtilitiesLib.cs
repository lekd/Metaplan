using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace PostIt_Prototype_1.Utilities
{
    public class UtilitiesLib
    {

        public static double distanceBetweenTwoPoints(float x1, float y1, float x2, float y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        public static PointF convertRelativeCoordinateToGlobalCoordinate(float relativeX, float relativeY, float screenW, float screenH)
        {
            PointF globalCoordinate = new PointF();
            globalCoordinate.X = relativeX * screenW;
            globalCoordinate.Y = relativeY * screenH;
            return globalCoordinate;
        }
        public static int Bytes2Int(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToInt32(bytes,0);
        }
        public static float Bytes2Float(byte[] bytes)
        {
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToSingle(bytes, 0);
        }
        public static BitmapImage convertBitmapToBitmapImage(Bitmap bmp)
        {
            MemoryStream ms = new MemoryStream();
            bmp.Save(ms,System.Drawing.Imaging.ImageFormat.Png);
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
    }
}
