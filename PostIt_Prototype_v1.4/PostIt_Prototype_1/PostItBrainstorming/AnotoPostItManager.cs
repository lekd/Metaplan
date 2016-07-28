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

        public event NewPostItAdded newNoteAddedHandler = null;
        public event PostItRemoved noteRemovedHandler = null;
        public event PostItContentUpdated noteContentUpdatedHandler = null;
        #endregion

        List<AnotoPostIt> anotoNotes = null;
        //at this moment assume the wallpaper/main canvas ID is 0
        int wallPaperID = 0;
        List<AnotoInkTrace> bufferedTraces = null;
        public AnotoPostItManager()
        {
            anotoNotes = new List<AnotoPostIt>();
            bufferedTraces = new List<AnotoInkTrace>();
        }
        public void addPostIt(AnotoPostIt note)
        {
            anotoNotes.Add(note);
        }
        private AnotoPostIt getPostItWithID(int id)
        {
		    foreach(var postIt in anotoNotes)
            {
			    if(postIt.Id==id)
                {
				    return postIt;
			    }
		    }
		    return null;
	    }
        public void processSingleIDTrace(AnotoInkTrace trace)
        {
            var postIt = getPostItWithID(trace.InkDots[0].PaperNoteID);
            if (postIt == null)
            {
                bufferedTraces.Add(trace);
            }
            else
            {
                if (postIt.IsAvailable)
                {
                    postIt.updateContent(trace);
                }
            }
        }
        public void processMultipleIDsTrace(AnotoInkTrace multiIDTrace)
        {
		    var singleIDTraces = multiIDTrace.splitToSingleIDTraces();
		    foreach(var trace in singleIDTraces)
            {
			    processSingleIDTrace(trace);
		    }
	    }
        public AnotoPostIt createNewPostItWithNoteID(int noteID)
        {
		    foreach(var postIt in anotoNotes){
			    //note with this ID already exists
			    if(postIt.Id==noteID){
				    return null;
			    }
		    }
		    var newPostIt = new AnotoPostIt(noteID);
		    for(var i=0;i<bufferedTraces.Count;)
            {
			    if(bufferedTraces[i].InkDots[0].PaperNoteID==noteID){
				    newPostIt.updateContent(bufferedTraces[i]);
                    bufferedTraces.RemoveAt(i);
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
            if (!generatedTrace.isComplete())
            {
                return;
            }
		    // TODO Auto-generated method stub
		    if(generatedTrace.isPotentialAssignGesture()){
			    //if the ID of the paper outside the note is not the wallpaper
			    //then it is not "assign" gesture
			    if(generatedTrace.InkDots[0].PaperNoteID!=wallPaperID){
				    processMultipleIDsTrace(generatedTrace);
			    }
			    else{
				    //add new PostIt from buffered traces
				    //first get the ID of the note to be created
				    // it should be different from the wallpaper
				    var postItID = 0;
				    foreach(var inkDot in generatedTrace.InkDots){
					    if(inkDot.PaperNoteID!=wallPaperID){
						    postItID = inkDot.PaperNoteID;
						    break;
					    }
				    }
				    var newPostIt = createNewPostItWithNoteID(postItID);
				    if(newPostIt!=null){
                        newPostIt.extractPositionFromAssigningTrace(generatedTrace);
                        newPostIt.postItRemovedHandler += new AnotoPostIt.DrawablePostItRemoved(newPostIt_postItRemovedHandler);
                        newPostIt.contentUpdatedHandler += new AnotoPostIt.DrawableContentUpdated(newPostIt_contentUpdatedHandler);
                        anotoNotes.Add(newPostIt);
                        if (newNoteAddedHandler != null)
                        {
                            newNoteAddedHandler(newPostIt);
                        }
				    }
				    else{
					    processMultipleIDsTrace(generatedTrace);
				    }
			    }
		    }
		    else{
			    if(generatedTrace.isMultiIDTrace()){
				    processMultipleIDsTrace(generatedTrace);
			    }
			    else{
				    processSingleIDTrace(generatedTrace);
			    }
		    }
	    }

        void newPostIt_contentUpdatedHandler(AnotoPostIt updatedNote)
        {
            if (noteContentUpdatedHandler != null)
            {
                noteContentUpdatedHandler(updatedNote);
            }
        }

        void newPostIt_postItRemovedHandler(AnotoPostIt removedNote)
        {
            removedNote.IsAvailable = false;
            if (noteRemovedHandler != null)
            {
                noteRemovedHandler(removedNote);
            }
        }
    }
}
