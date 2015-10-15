using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.Presentation
{
    public interface IPostItUI
    {
        int getNoteID();
        void setNoteID(int id);
        void updateDisplayedContent(object content);
    }
}
