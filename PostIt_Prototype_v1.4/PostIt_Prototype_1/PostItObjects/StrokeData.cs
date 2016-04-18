using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;

namespace PostIt_Prototype_1.PostItObjects
{
    public class StrokeData
    {
        bool _isErasingStroke;

        public bool IsErasingStroke
        {
            get { return _isErasingStroke; }
            set { _isErasingStroke = value; }
        }
        List<Point> _strokePoints;

        public List<Point> StrokePoints
        {
            get { return _strokePoints; }
            set { _strokePoints = value; }
        }
        string _strokeColorCode;

        public string StrokeColorCode
        {
            get { return _strokeColorCode; }
            set { _strokeColorCode = value; }
        }
        public StrokeData()
        {
            _isErasingStroke = false;
            _strokePoints = new List<Point>();
            _strokeColorCode = new System.Windows.Media.ColorConverter().ConvertToString(System.Windows.Media.Color.FromRgb(255, 255, 255));
        }
        public void ParseIsErasingFromString(string isErasingString)
        {
            try
            {
                _isErasingStroke = Boolean.Parse(isErasingString);
            }
            catch
            {
                _isErasingStroke = false;
            }
        }
        public void ParseStrokePointsFromString(string strokeDataString)
        {
            try
            {
                _strokePoints.Clear();
                string[] pointStrs = strokeDataString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string pStr in pointStrs)
                {
                    string[] pComponents = pStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (pComponents.Length == 2)
                    {
                        Point p = new Point();
                        p.X = Double.Parse(pComponents[0], CultureInfo.InvariantCulture);
                        p.Y = Double.Parse(pComponents[1], CultureInfo.InvariantCulture);
                        _strokePoints.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(
                                "StrokeData-ParseStrokePointsFromString: ", ex);
            }
            
            
        }
        public string getStringFromStrokePoints()
        {
            string contentStr = string.Empty;
            try
            {
                for (int i = 0; i < _strokePoints.Count; i++)
                {
                    string pointStr = string.Format("{0},{1}", _strokePoints[i].X, _strokePoints[i].Y);
                    if (i < _strokePoints.Count - 1)
                    {
                        pointStr += ";";
                    }
                    contentStr += pointStr;
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(
                                        "StrokeData-getStringFromStrokePoints: ", ex);
            }
            
            return contentStr;
        }
        public string getStringOfIsErasingAttribute()
        {
            return _isErasingStroke.ToString();
        }
    }
}
