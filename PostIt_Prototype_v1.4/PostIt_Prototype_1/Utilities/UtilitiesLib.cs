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

        public static double distanceBetweenTwoPoints(double x1, double y1, double x2, double y2)
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
        //path related functions
        public static bool CheckClosedPath(List<System.Windows.Point> path)
        {
            System.Windows.Point beginning = path[0];
            System.Windows.Point end = path[path.Count - 1];
            double beginToEnd = distanceBetweenTwoPoints(beginning.X, beginning.Y, end.X, end.Y);
            double totalLength = 0;
            for (int i = 0; i < path.Count - 1; i++)
            {
                System.Windows.Point current = path[i];
                System.Windows.Point next = path[i + 1];
                totalLength += distanceBetweenTwoPoints(current.X, current.Y, next.X, next.Y);
            }
            if (beginToEnd > 100)
            {
                return false;
            }
            if (beginToEnd == 0 && totalLength != 0)
            {
                return true;
            }
            if (totalLength / beginToEnd < 10)
            {
                return false;
            }
            return true;
        }
        public static bool InsidePolygon(List<System.Windows.Point> polygon, System.Windows.Point p)
        {
            bool result = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                System.Windows.Point p1 = polygon[i];
                System.Windows.Point p2 = polygon[j];
                if (((p1.Y > p.Y) != (p2.Y > p.Y))
                    || (p.X < (p2.X - p1.X) * (p.Y - p1.Y) / (p2.Y - p1.Y + p1.X)))
                {
                    result = !result;
                }
            }
            return result;
        }
        public static List<System.Windows.Point> shiftPathToCoordinateOrigin(List<System.Windows.Point> path)
        {
            List<System.Windows.Point> shiftedPath = new List<System.Windows.Point>(path);
            System.Windows.Point topleft, bottomright, center;
            extractAnchorPointsOfPath(shiftedPath,out topleft,out bottomright,out center);
            for (int i = 0; i < shiftedPath.Count; i++)
            {
                System.Windows.Point p = shiftedPath[i];
                p.X -= topleft.X;
                p.Y -= topleft.Y;
                shiftedPath[i] = p;
            }
            return shiftedPath;
        }
        static public void writeToFileToDebug(string filePath, string content)
        {
            StreamWriter file = new StreamWriter(filePath, true);
            file.WriteLine(content);
            file.Close();
        }
        public static void extractAnchorPointsOfPath(List<System.Windows.Point> path, out System.Windows.Point topleft, out System.Windows.Point bottomright, out System.Windows.Point center)
        {
            double top = double.MaxValue;
            double left = double.MaxValue;
            double bottom = double.MinValue;
            double right = double.MinValue;
            double centerX = 0;
            double centerY = 0;
            for (int i = 0; i < path.Count; i++)
            {
                System.Windows.Point p = path[i];
                if (p.X < left)
                {
                    left = p.X;
                }
                if (p.X > right)
                {
                    right = p.X;
                }

                if (p.Y < top)
                {
                    top = p.Y;
                }
                if (p.Y > bottom)
                {
                    bottom = p.Y;
                }
            }
            centerX = (left + right) / 2;
            centerY = (top + bottom) / 2;
            topleft = new System.Windows.Point(left, top);
            bottomright = new System.Windows.Point(right, bottom);
            center = new System.Windows.Point(centerX, centerY);
        }
    }
}
