using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using GenericIdeationObjects;

namespace PostIt_Prototype_1.PostItObjects
{
    public class StrokeBasedIdea:GenericIdeationObjects.IdeationUnit
    {
        public StrokeBasedIdea()
        {
            _content = null;
            _isAvailable = true;
        }
        public static string getDataStringOfStrokeContent(object strokeContent)
        {
            List<Point> strokePoints = (List<Point>)strokeContent;
            string contentStr = string.Empty;
            for (int i = 0; i < strokePoints.Count; i++)
            {
                string pointStr = string.Format("{0},{1}", strokePoints[i].X, strokePoints[i].Y);
                if (i < strokePoints.Count - 1)
                {
                    pointStr += ";";
                }
                contentStr += pointStr;
            }
            return contentStr;
        }
        public static object ParseContentFromString(string strokeDataString)
        {
            List<Point> strokePoints = new List<Point>();
            string[] pointStrs = strokeDataString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string pStr in pointStrs)
            {
                string[] pComponents = pStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                if (pComponents.Length == 2)
                {
                    Point p = new Point();
                    p.X = Double.Parse(pComponents[0], CultureInfo.InvariantCulture);
                    p.Y = Double.Parse(pComponents[1], CultureInfo.InvariantCulture);
                    strokePoints.Add(p);
                }
            }
            return strokePoints;
        }
        public override IdeationUnit Clone()
        {
            IdeationUnit clone = new StrokeBasedIdea();
            clone.Id = _id;
            clone.IsAvailable = _isAvailable;
            List<Point> strokePoints = new List<Point>((List<Point>)_content);
            return clone;
        }
    }
}
