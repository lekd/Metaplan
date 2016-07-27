using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PostIt_Prototype_1.PostItDataHandlers;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.PostItBrainstorming
{
    public class AnotoPostItManager
    {
        #region Custom Events 
        public delegate void NewPostItAdded(AnotoPostIt newPostIt);
        public delegate void PostItRemoved(AnotoPostIt removedPostIt);
        public delegate void PostItContentUpdated(AnotoPostIt updatedPostIt);

        public event NewPostItAdded NewNoteAddedHandler = null;
        public event PostItRemoved NoteRemovedHandler = null;
        public event PostItContentUpdated NoteContentUpdatedHandler = null;
        #endregion

        List<AnotoPostIt> _anotoNotes = null;
        //at this moment assume the wallpaper/main canvas ID is 0
        int _wallPaperId = 0;
        List<AnotoInkTrace> _bufferedTraces = null;
        public AnotoPostItManager()
        {
            _anotoNotes = new List<AnotoPostIt>();
            _bufferedTraces = new List<AnotoInkTrace>();
        }
        public void AddPostIt(AnotoPostIt note)
        {
            _anotoNotes.Add(note);
        }
        private AnotoPostIt GetPostItWithId(int id)
        {
		    foreach(var postIt in _anotoNotes)
            {
			    if(postIt.Id==id)
                {
				    return postIt;
			    }
		    }
		    return null;
	    }
        public void ProcessSingleIdTrace(AnotoInkTrace trace)
        {
            var postIt = GetPostItWithId(trace.InkDots[0].PaperNoteId);
            if (postIt == null)
            {
                _bufferedTraces.Add(trace);
            }
            else
            {
                if (postIt.IsAvailable)
                {
                    postIt.UpdateContent(trace);
                }
            }
        }
        public void ProcessMultipleIDsTrace(AnotoInkTrace multiIdTrace)
        {
		    var singleIdTraces = multiIdTrace.SplitToSingleIdTraces();
		    foreach(var trace in singleIdTraces)
            {
			    ProcessSingleIdTrace(trace);
		    }
	    }
        public AnotoPostIt CreateNewPostItWithNoteId(int noteId)
        {
		    foreach(var postIt in _anotoNotes){
			    //note with this ID already exists
			    if(postIt.Id==noteId){
				    return null;
			    }
		    }
		    var newPostIt = new AnotoPostIt(noteId);
		    for(var i=0;i<_bufferedTraces.Count;)
            {
			    if(_bufferedTraces[i].InkDots[0].PaperNoteId==noteId){
				    newPostIt.UpdateContent(_bufferedTraces[i]);
                    _bufferedTraces.RemoveAt(i);
			    }
			    else{
				    i++;
			    }
		    }
            newPostIt.IsAvailable = true;
		    return newPostIt;
	    }
        public void traceGeneratedHandler(AnotoInkTrace generatedTrace) 
        {
            if (!generatedTrace.IsComplete())
            {
                return;
            }
		    // TODO Auto-generated method stub
		    if(generatedTrace.IsPotentialAssignGesture()){
			    //if the ID of the paper outside the note is not the wallpaper
			    //then it is not "assign" gesture
			    if(generatedTrace.InkDots[0].PaperNoteId!=_wallPaperId){
				    ProcessMultipleIDsTrace(generatedTrace);
			    }
			    else{
				    //add new PostIt from buffered traces
				    //first get the ID of the note to be created
				    // it should be different from the wallpaper
				    var postItId = 0;
				    foreach(var inkDot in generatedTrace.InkDots){
					    if(inkDot.PaperNoteId!=_wallPaperId){
						    postItId = inkDot.PaperNoteId;
						    break;
					    }
				    }
				    var newPostIt = CreateNewPostItWithNoteId(postItId);
				    if(newPostIt!=null){
                        newPostIt.ExtractPositionFromAssigningTrace(generatedTrace);
                        newPostIt.PostItRemovedHandler += new AnotoPostIt.DrawablePostItRemoved(newPostIt_postItRemovedHandler);
                        newPostIt.ContentUpdatedHandler += new AnotoPostIt.DrawableContentUpdated(newPostIt_contentUpdatedHandler);
                        _anotoNotes.Add(newPostIt);
                        if (NewNoteAddedHandler != null)
                        {
                            NewNoteAddedHandler(newPostIt);
                        }
				    }
				    else{
					    ProcessMultipleIDsTrace(generatedTrace);
				    }
			    }
		    }
		    else{
			    if(generatedTrace.IsMultiIdTrace()){
				    ProcessMultipleIDsTrace(generatedTrace);
			    }
			    else{
				    ProcessSingleIdTrace(generatedTrace);
			    }
		    }
	    }

        void newPostIt_contentUpdatedHandler(AnotoPostIt updatedNote)
        {
            if (NoteContentUpdatedHandler != null)
            {
                NoteContentUpdatedHandler(updatedNote);
            }
        }

        void newPostIt_postItRemovedHandler(AnotoPostIt removedNote)
        {
            removedNote.IsAvailable = false;
            if (NoteRemovedHandler != null)
            {
                NoteRemovedHandler(removedNote);
            }
        }
    }
}
