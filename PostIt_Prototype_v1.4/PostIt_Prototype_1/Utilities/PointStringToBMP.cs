using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PostIt_Prototype_1.Utilities
{
    public class PointStringToBMP
    {
        float Scale = 1.0f;
        public PointStringToBMP(float scale = 1.0f)
		{	
			Scale = scale;		
		}

		public Bitmap FromString(String fullStr, int width, int height, int left, int top)
		{			
			var lines = fullStr.Split ('\n');
			var bmp = new Bitmap (width, height);
			var drawing = Graphics.FromImage (bmp);
			drawing.FillRectangle (Brushes.White, 0, 0, width, height);

			foreach (var str in lines) {
				if (str == "")
					continue;
				var strPoints = str.Split (' ');
				var numberOfPoints = (strPoints.Length - 1) / 2;
				// Last substr is always null string
				var points = new PointF[numberOfPoints];

				for (var i = 0; i < numberOfPoints; i++)
					points [i] = new PointF (float.Parse (strPoints [2 * i]) / this.Scale - left, float.Parse (strPoints [2 * i + 1]) / this.Scale - top);

				drawing.DrawLines(Pens.Black, points);
			}
			return bmp;
		}

        

    }
}
