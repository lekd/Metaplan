using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace TextToBMP
{
	public class Text2BMP
	{		
		float Scale = 1.0f;
		public Text2BMP (float scale = 1.0f)
		{	
			Scale = scale;		
		}

		public Bitmap FromString(String fullStr, int width, int height)
		{			
			var lines = fullStr.Split ('\n');
			Console.WriteLine ("Number of lines= {0}", lines.Length);
			var bmp = new Bitmap (width, height);
			var drawing = Graphics.FromImage (bmp);
			drawing.FillRectangle (Brushes.White, 0, 0, width, height);

			foreach (var str in lines) {
				if (str == "")
					continue;
				var strPoints = str.Split (' ');
				var numberOfPoints = (strPoints.Length - 1) / 2;
				Console.WriteLine (strPoints);
				Console.WriteLine (numberOfPoints);
				// Last substr is always null string
				var points = new PointF[numberOfPoints];

				for (int i = 0; i < numberOfPoints; i++)
					points [i] = new PointF (float.Parse (strPoints [2 * i]) / this.Scale, float.Parse (strPoints [2 * i + 1]) / this.Scale);

				drawing.DrawLines(Pens.Black, points);
			}
			return bmp;
		}
	}
}

