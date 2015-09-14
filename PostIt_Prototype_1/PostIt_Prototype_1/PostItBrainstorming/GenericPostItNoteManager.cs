using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostIt_Prototype_1.PostItDataHandlers;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.PostItBrainstorming
{
    public class GenericPostItNoteManager
    {
        public delegate void NewPostItAdded(PostItNote newPostIt);
        public delegate void PostItRemoved(PostItNote removedPostIt);
        public delegate void PostItUpdated(PostItNote updatedPostIt);

        public event NewPostItAdded noteAddedEventHandler = null;
        public event PostItRemoved noteRemovedEventHandler = null;
        public event PostItUpdated noteUpdatedEventHandler = null;

        List<PostItNote> postItNotes = null;
        public GenericPostItNoteManager()
        {
            postItNotes = new List<PostItNote>();
        }
        PostItNote getNoteWithID(int noteID)
        {
            foreach (PostItNote note in postItNotes)
            {
                if (note.Id == noteID)
                {
                    return note;
                }
            }
            return null;
        }
        public void ProcessPostItNoteCommand(PostItCommandType commandType, object commandArg)
        {
            switch (commandType)
            {
                case PostItCommandType.Add:
                    PostItNote addedNote = (PostItNote)commandArg;
                    if (getNoteWithID(addedNote.Id) == null)
                    {
                        postItNotes.Add(addedNote);
                    }
                    if (noteAddedEventHandler != null)
                    {
                        noteAddedEventHandler(addedNote);
                    }
                    break;
                case PostItCommandType.Update:
                    PostItNote updatedNote = (PostItNote)commandArg;
                    PostItNote matchingNote = getNoteWithID(updatedNote.Id);
                    if (!(matchingNote == null))
                    {
                        matchingNote.Left = updatedNote.Left;
                        matchingNote.Top = updatedNote.Top;
                        if (updatedNote.Content != null)
                        {
                            matchingNote.Content = updatedNote.Content;
                        }
                        if (noteUpdatedEventHandler != null)
                        {
                            noteUpdatedEventHandler(matchingNote);
                        }
                    }
                    break;
                case PostItCommandType.Delete:
                    int noteID = (int)commandArg;
                    PostItNote tobeRemoved = getNoteWithID(noteID);
                    if (tobeRemoved != null)
                    {
                        postItNotes.Remove(tobeRemoved);
                        if (noteRemovedEventHandler != null)
                        {
                            noteRemovedEventHandler(tobeRemoved);
                        }
                    }
                    break;
            }
        }
    }
}
