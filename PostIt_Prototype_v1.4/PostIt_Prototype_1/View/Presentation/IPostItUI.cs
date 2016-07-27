using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace PostIt_Prototype_1.Presentation
{
    public delegate void NoteUiTranslatedEvent(object sender, GenericIdeationObjects.IdeationUnit associatedIdea, float newX, float newY);
    public delegate void NoteUiDeletedEvent(object sender, GenericIdeationObjects.IdeationUnit associatedIdea);
    public delegate void NoteUiSizeChangedEvent(object sender, GenericIdeationObjects.IdeationUnit associatedIdea, float scaleX,float scaleY);
    public delegate void ColorPaletteLaunchedEvent(object sender,float posX,float posY);
    public interface IPostItUi
    {
        event NoteUiTranslatedEvent NoteUiTranslatedEventHandler;
        event NoteUiDeletedEvent NoteUiDeletedEventHandler;
        event NoteUiSizeChangedEvent NoteUiSizeChangedListener;
        event ColorPaletteLaunchedEvent ColorPaletteLaunchedEventHandler;
        GenericIdeationObjects.IdeationUnit GetAssociatedIdea();
        Control Container { get; set; }
        int GetNoteId();
        void SetNoteId(int id);
        void Update(GenericIdeationObjects.IdeationUnit idea);
        void UpdateDisplayedContent(object content);
        void InitContainer(Control container);
        void StartJustAddedAnimation(double initRotation);
    }
}
