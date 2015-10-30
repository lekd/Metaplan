using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;

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
        public static byte[] BitmapToBytes(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public static byte[] BitmapImageToBytes(BitmapImage bitmapImage)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                PngBitmapEncoder imageEncoder = new PngBitmapEncoder();
                imageEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                imageEncoder.Save(stream);
                return stream.ToArray();
            }
            return null;
        }
        public static Bitmap BytesToBitmap(byte[] bytes)
        {
            Bitmap bmp = null;
            using (var ms = new MemoryStream(bytes))
            {
                bmp = new Bitmap(ms);
            }
            return bmp;
        }
        public static BitmapFrame CreateResizedBitmapFrame(ImageSource src, int width, int height, int margin)
        {
            var rect = new Rect(margin, margin, width - margin * 2, height - margin * 2);

            var group = new DrawingGroup();
            RenderOptions.SetBitmapScalingMode(group, BitmapScalingMode.HighQuality);
            group.Children.Add(new ImageDrawing(src, rect));

            var drawingVisual = new DrawingVisual();
            using (var drawingContext = drawingVisual.RenderOpen())
                drawingContext.DrawDrawing(group);

            var resizedImage = new RenderTargetBitmap(
                width, height,         // Resized dimensions
                96, 96,                // Default DPI values
                PixelFormats.Default); // Default pixel format
            resizedImage.Render(drawingVisual);

            return BitmapFrame.Create(resizedImage);
        }
    }
}
