using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenericIdeationObjects
{
    public class IdeationUnitGroup : IdeationUnit
    {
        protected List<IdeationUnit> _childrenUnits;

        public List<IdeationUnit> ChildrenUnits
        {
            get { return _childrenUnits; }
            set { _childrenUnits = value; }
        }
        protected GroupMetaData _metaData;

        public GroupMetaData MetaData
        {
            get { return _metaData; }
            set { _metaData = value; }
        }

        public IdeationUnitGroup()
        {
            _childrenUnits = new List<IdeationUnit>();

        }
    }
}
