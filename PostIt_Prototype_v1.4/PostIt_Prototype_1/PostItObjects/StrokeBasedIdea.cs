using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Globalization;
using GenericIdeationObjects;
using System.Windows.Media;

namespace WhiteboardApp.PostItObjects
{
    public class StrokeBasedIdea:GenericIdeationObjects.IdeationUnit
    {
        
        public StrokeBasedIdea()
        {
            _content = null;
            _isAvailable = true;
        }
        
        public override IdeationUnit Clone()
        {
            IdeationUnit clone = new StrokeBasedIdea();
            clone.Id = _id;
            clone.IsAvailable = _isAvailable;
            clone.Content = _content;
            return clone;
        }
    }
}
