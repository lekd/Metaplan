using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PostIt_Prototype_1.Presentation
{
    public delegate void NoteUITranslatedEvent(object sender, GenericIdeationObjects.IdeationUnit associatedIdea, float newX, float newY);
    public delegate void NoteUIDeletedEvent(object sender, GenericIdeationObjects.IdeationUnit associatedIdea);
    public interface IPostItUI
    {
        event NoteUITranslatedEvent noteUITranslatedEventHandler;
        event NoteUIDeletedEvent noteUIDeletedEventHandler;
        GenericIdeationObjects.IdeationUnit getAssociatedIdea();
        Control Container { get; set; }
        int getNoteID();
        void setNoteID(int id);
        void update(GenericIdeationObjects.IdeationUnit idea);
        void updateDisplayedContent(object content);
        void InitContainer(Control container);
        void startJustAddedAnimation(double initRotation);
    }
}
