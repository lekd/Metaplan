using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using WhiteboardApp.PostItDataHandlers;
using WhiteboardApp.PostItObjects;

namespace WhiteboardApp.PostItBrainstorming
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
            foreach (var note in postItNotes)
            {
                if (note.Id == noteID)
                {
                    return note;
                }
            }
            return null;
        }
        public void addNote(PostItNote note)
        {
            if (getNoteWithID(note.Id)==null)
            {
                postItNotes.Add(note);
                if (noteAddedEventHandler != null)
                {
                    noteAddedEventHandler(note);
                }
            }
            else
            {
                var existingNote = getNoteWithID(note.Id);
                if (!(existingNote.IsAvailable))
                {
                    existingNote.IsAvailable = true;
                    existingNote.CenterX = 0;
                    existingNote.CenterY = 0;
                    existingNote.Content = note.Content;
                    existingNote.DataType = note.DataType;
                    if (noteAddedEventHandler != null)
                    {
                        noteAddedEventHandler(existingNote);
                    }
                }
                else
                {
                    existingNote.Content = note.Content;
                    if (noteUpdatedEventHandler != null)
                    {
                        noteUpdatedEventHandler(existingNote);
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
                    var updatedNote = (PostItNote)commandArg;
                    var matchingNote = getNoteWithID(updatedNote.Id);
                    if (!(matchingNote == null))
                    {
                        matchingNote.CenterX = updatedNote.CenterX;
                        matchingNote.CenterX = updatedNote.CenterY;
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
                    var noteID = (int)commandArg;
                    var tobeRemoved = getNoteWithID(noteID);
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
