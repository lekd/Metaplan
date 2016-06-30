using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItObjects
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
