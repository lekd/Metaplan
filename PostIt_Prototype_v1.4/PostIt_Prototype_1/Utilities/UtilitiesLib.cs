using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Effects;
using Google.Apis.Drive.v3;
using Google.Apis.Drive.v3.Data;

namespace WhiteboardApp.Utilities
{
    public static class UtilitiesLib
    {

        public static double distanceBetweenTwoPoints(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }
        public static PointF convertRelativeCoordinateToGlobalCoordinate(float relativeX, float relativeY, float screenW, float screenH)
        {
            var globalCoordinate = new PointF();
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
            var ms = new MemoryStream();
            bmp.Save(ms,System.Drawing.Imaging.ImageFormat.Png);
            var image = new BitmapImage();
            image.BeginInit();
            ms.Seek(0, SeekOrigin.Begin);
            image.StreamSource = ms;
            image.EndInit();
            return image;
        }
        public static byte[] BitmapToBytes(Image img)
        {
            var converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
        public static byte[] BitmapImageToBytes(BitmapImage bitmapImage)
        {
            using (var stream = new MemoryStream())
            {
                var imageEncoder = new PngBitmapEncoder();
                imageEncoder.Frames.Add(BitmapFrame.Create(bitmapImage));
                imageEncoder.Save(stream);
                return stream.ToArray();
            }
        }

        public static void TerminateWithError(string message)
        {
            MessageBox.Show($"{message}\r\nPlease contact the support team. Program will terminate.", "MERCO Brainstorming", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(-1);
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
            var beginning = path[0];
            var end = path[path.Count - 1];
            var beginToEnd = distanceBetweenTwoPoints(beginning.X, beginning.Y, end.X, end.Y);
            double totalLength = 0;
            for (var i = 0; i < path.Count - 1; i++)
            {
                var current = path[i];
                var next = path[i + 1];
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
            var result = false;
            for (int i = 0, j = polygon.Count - 1; i < polygon.Count; j = i++)
            {
                var p1 = polygon[i];
                var p2 = polygon[j];
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
            var shiftedPath = new List<System.Windows.Point>(path);
            System.Windows.Point topleft, bottomright, center;
            extractAnchorPointsOfPath(shiftedPath,out topleft,out bottomright,out center);
            for (var i = 0; i < shiftedPath.Count; i++)
            {
                var p = shiftedPath[i];
                p.X -= topleft.X;
                p.Y -= topleft.Y;
                shiftedPath[i] = p;
            }
            return shiftedPath;
        }
        static public void writeToFileToDebug(string filePath, string content)
        {
            try
            {
                var contentWithTimeStamp = DateTime.Now.ToString() + "--" + content;
                var file = new StreamWriter(filePath, true);
                file.WriteLine(contentWithTimeStamp);
                file.Flush();
                file.Close();
            }
            catch { }
        }
        public static void extractAnchorPointsOfPath(List<System.Windows.Point> path, out System.Windows.Point topleft, out System.Windows.Point bottomright, out System.Windows.Point center)
        {
            var top = double.MaxValue;
            var left = double.MaxValue;
            var bottom = double.MinValue;
            var right = double.MinValue;
            double centerX = 0;
            double centerY = 0;
            for (var i = 0; i < path.Count; i++)
            {
                var p = path[i];
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

        public static void LogError(Exception ex)
        {
                #if DEBUG
                            //MessageBox.Show(ex.StackTrace);
                #endif

            Utilities.UtilitiesLib.writeToFileToDebug(Properties.Settings.Default.DebugLogFile, ex.ToString());
        }
        public static DropShadowBitmapEffect getShadowEffect()
        {
            
            var shadowEffect = new DropShadowBitmapEffect();
            var myShadowColor = new System.Windows.Media.Color();
            myShadowColor.ScA = 1;
            myShadowColor.ScR = 0;
            myShadowColor.ScG = 0;
            myShadowColor.ScB = 0;
            shadowEffect.Color = myShadowColor;
            shadowEffect.Direction = 320;
            shadowEffect.ShadowDepth = 10;
            shadowEffect.Softness = 1;
            shadowEffect.Opacity = 0.3;
            return shadowEffect;
        }

    }
}
