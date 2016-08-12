using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;

namespace WhiteboardApp.PostItObjects
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
                var pointStrs = strokeDataString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var pStr in pointStrs)
                {
                    var pComponents = pStr.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                    if (pComponents.Length == 2)
                    {
                        var p = new Point();
                        p.X = Double.Parse(pComponents[0], CultureInfo.InvariantCulture);
                        p.Y = Double.Parse(pComponents[1], CultureInfo.InvariantCulture);
                        _strokePoints.Add(p);
                    }
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            
            
        }
        public string getStringFromStrokePoints()
        {
            var contentStr = string.Empty;
            try
            {
                for (var i = 0; i < _strokePoints.Count; i++)
                {
                    string pointStr = $"{_strokePoints[i].X},{_strokePoints[i].Y}";
                    if (i < _strokePoints.Count - 1)
                    {
                        pointStr += ";";
                    }
                    contentStr += pointStr;
                }
            }
            catch (Exception ex)
            {
                Utilities.UtilitiesLib.LogError(ex);
            }
            
            return contentStr;
        }
        public string getStringOfIsErasingAttribute()
        {
            return _isErasingStroke.ToString();
        }
    }
}
