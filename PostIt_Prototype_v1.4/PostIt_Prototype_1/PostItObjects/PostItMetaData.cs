using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WhiteboardApp.PostItObjects
{
    public class PostItMetaData:GenericIdeationObjects.IdeationUnitMetaData
    {
        string _uiBackgroundColor;

        public string UiBackgroundColor
        {
            get { return _uiBackgroundColor; }
            set { _uiBackgroundColor = value; }
        }
        public PostItMetaData()
        {
            _uiBackgroundColor = "";
        }
    }
}
