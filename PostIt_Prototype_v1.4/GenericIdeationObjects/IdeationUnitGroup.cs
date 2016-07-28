using System;
using System.Collections.Generic;
using System.Text;

namespace GenericIdeationObjects
{
    public class IdeationUnitGroup : IdeationUnit
    {
        
        public IdeationUnitGroup()
        {
            _content = new IdeaGroupContentType();
            _isAvailable = true;
        }
    }
}
