using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace GenericIdeationObjects
{
    public class IdeaGroupContentType
    {
        List<int> _childrenIdeaIDs;

        public List<int> ChildrenIdeaIDs
        {
            get { return _childrenIdeaIDs; }
            set { _childrenIdeaIDs = value; }
        }
        List<Point> _displayBoundaries;

        public List<Point> DisplayBoundaries
        {
            get { return _displayBoundaries; }
            set { _displayBoundaries = value; }
        }
        public IdeaGroupContentType()
        {
            _childrenIdeaIDs = new List<int>();
            _displayBoundaries = new List<Point>();
        }
        public string getStringOfChildrenIDs()
        {
            string idStr = string.Empty;
            for (int i = 0; i < _childrenIdeaIDs.Count; i++)
            {
                idStr += _childrenIdeaIDs[i].ToString();
                if (i < ChildrenIdeaIDs.Count - 1)
                {
                    idStr += ",";
                }
            }
            
            return idStr;
        }
        public string getStringOfBoundaryPoints()
        {
            string boundaryStr = string.Empty;
            for (int i = 0; i < _displayBoundaries.Count; i++)
            {
                string pStr = $"{_displayBoundaries[i].X},{_displayBoundaries[i].Y}";
                if (i < _displayBoundaries.Count - 1)
                {
                    pStr += ";";
                }
                boundaryStr += pStr;
            }
            return boundaryStr;
        }
    }
}
