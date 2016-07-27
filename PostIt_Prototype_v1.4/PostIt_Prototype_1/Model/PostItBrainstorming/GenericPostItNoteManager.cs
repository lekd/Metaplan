using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostIt_Prototype_1.PostItDataHandlers;
using PostIt_Prototype_1.PostItObjects;
using System.IO;
using PostIt_Prototype_1.Model.PostItDataHandlers;
using PostIt_Prototype_1.ModelView.PostItObjects;

namespace PostIt_Prototype_1.PostItBrainstorming
{
    public class GenericPostItNoteManager
    {
        public delegate void NewPostItAdded(PostItNote newPostIt);
        public delegate void PostItRemoved(PostItNote removedPostIt);
        public delegate void PostItUpdated(PostItNote updatedPostIt);

        public event NewPostItAdded NoteAddedEventHandler = null;
        public event PostItRemoved NoteRemovedEventHandler = null;
        public event PostItUpdated NoteUpdatedEventHandler = null;

        List<PostItNote> _postItNotes = null;
        public GenericPostItNoteManager()
        {
            _postItNotes = new List<PostItNote>();
        }
        PostItNote GetNoteWithId(int noteId)
        {
            foreach (var note in _postItNotes)
            {
                if (note.Id == noteId)
                {
                    return note;
                }
            }
            return null;
        }
        public void AddNote(PostItNote note)
        {
            if (GetNoteWithId(note.Id)==null)
            {
                _postItNotes.Add(note);
                if (NoteAddedEventHandler != null)
                {
                    NoteAddedEventHandler(note);
                }
            }
            else
            {
                var existingNote = GetNoteWithId(note.Id);
                if (!(existingNote.IsAvailable))
                {
                    existingNote.IsAvailable = true;
                    existingNote.CenterX = 0;
                    existingNote.CenterY = 0;
                    existingNote.Content = note.Content;
                    existingNote.DataType = note.DataType;
                    if (NoteAddedEventHandler != null)
                    {
                        NoteAddedEventHandler(existingNote);
                    }
                }
                else
                {
                    existingNote.Content = note.Content;
                    if (NoteUpdatedEventHandler != null)
                    {
                        NoteUpdatedEventHandler(existingNote);
                    }
                }
            }
        }
        public void ProcessPostItNoteCommand(PostItCommandType commandType, object commandArg)
        {
            switch (commandType)
            {
                case PostItCommandType.Add:
                    var addedNote = (PostItNote)commandArg;
                    if (GetNoteWithId(addedNote.Id) == null)
                    {
                        _postItNotes.Add(addedNote);
                    }
                    if (NoteAddedEventHandler != null)
                    {
                        NoteAddedEventHandler(addedNote);
                    }
                    break;
                case PostItCommandType.Update:
                    var updatedNote = (PostItNote)commandArg;
                    var matchingNote = GetNoteWithId(updatedNote.Id);
                    if (!(matchingNote == null))
                    {
                        matchingNote.CenterX = updatedNote.CenterX;
                        matchingNote.CenterX = updatedNote.CenterY;
                        if (updatedNote.Content != null)
                        {
                            matchingNote.Content = updatedNote.Content;
                        }
                        if (NoteUpdatedEventHandler != null)
                        {
                            NoteUpdatedEventHandler(matchingNote);
                        }
                    }
                    break;
                case PostItCommandType.Delete:
                    var noteId = (int)commandArg;
                    var tobeRemoved = GetNoteWithId(noteId);
                    if (tobeRemoved != null)
                    {
                        _postItNotes.Remove(tobeRemoved);
                        if (NoteRemovedEventHandler != null)
                        {
                            NoteRemovedEventHandler(tobeRemoved);
                        }
                    }
                    break;
            }
        }
    }
}
