using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using PostIt_Prototype_1.ModelView.PostItObjects;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.TimelineControllers
{
    public class TimlineEventIntepreter
    {
        public delegate void AddIdeaCommandExtracted(GenericIdeationObjects.IdeationUnit idea);
        public delegate void RemoveIdeaCommandExtracted(GenericIdeationObjects.IdeationUnit idea);
        public delegate void RestoreIdeaCommandExtraced(GenericIdeationObjects.IdeationUnit idea);
        public delegate void UpdateIdeaPositionCommandExtracted(int ideaId, float newX, float newY);
        public delegate void UpdateIdeaContentCommandExtracted(GenericIdeationObjects.IdeationUnit idea);
        public delegate void ColorChangeCommandExtracted(int ideaId, string colorCode);

        public event AddIdeaCommandExtracted AdDeventExtractedHandler = null;
        public event RemoveIdeaCommandExtracted RemovEeventExtractedHandler = null;
        public event RestoreIdeaCommandExtraced RestorEeventExtractedHandler = null;
        public event UpdateIdeaPositionCommandExtracted UpdatePosEventExtractedHandler = null;
        public event UpdateIdeaContentCommandExtracted UpdateContentEventExtractedHandler = null;
        public event ColorChangeCommandExtracted ColorChangeEventExtractedHandler = null;
        public void IntepretEvent(TimelineChange changeEvent)
        {
            switch (changeEvent.ChangeType)
            {
                case TypeOfChange.Add:
                    ExtractAdDevent(changeEvent);
                    break;
                case TypeOfChange.Delete:
                    ExtractRemovEevent(changeEvent);
                    break;
                case TypeOfChange.Restore:
                    ExtractRestorEevent(changeEvent);
                    break;
                case TypeOfChange.Update:
                    ExtractUpdatEevent(changeEvent);
                    break;
                case TypeOfChange.Color:
                    ExtractColorChangeEvent(changeEvent);
                    break;
            }
        }
        void ExtractAdDevent(TimelineChange changeEvent)
        {
            GenericIdeationObjects.IdeationUnit idea = new PostItNote();
            //this is a short-term solution, in the future need to re-implemented more sustainably
            if (changeEvent.MetaData is StrokeData)//this is the addition of a stroke
            {
                idea = new PostItObjects.StrokeBasedIdea();
            }
            idea.Id = changeEvent.ChangedIdeaId;
            idea.CenterX = 0;
            idea.CenterY = 0;
            idea.Content = changeEvent.MetaData;
            if (AdDeventExtractedHandler != null)
            {
                AdDeventExtractedHandler(idea);
            }
        }
        void ExtractRemovEevent(TimelineChange changeEvent)
        {
            GenericIdeationObjects.IdeationUnit idea = new PostItNote();
            idea.Id = changeEvent.ChangedIdeaId;
            if (RemovEeventExtractedHandler != null)
            {
                RemovEeventExtractedHandler(idea);
            }
        }
        void ExtractRestorEevent(TimelineChange changeEvent)
        {
            GenericIdeationObjects.IdeationUnit idea = new PostItNote();
            idea.Id = changeEvent.ChangedIdeaId;
            if (RemovEeventExtractedHandler != null)
            {
                RestorEeventExtractedHandler(idea);
            }
        }
        void ExtractUpdatEevent(TimelineChange changeEvent)
        {
            if (changeEvent.MetaData is Point)
            {
                if (UpdatePosEventExtractedHandler != null)
                {
                    var newPos = (Point)changeEvent.MetaData;
                    UpdatePosEventExtractedHandler(changeEvent.ChangedIdeaId, (float)newPos.X, (float)newPos.Y);
                }
            }
            else
            {
                if (UpdateContentEventExtractedHandler != null)
                {
                    GenericIdeationObjects.IdeationUnit idea = new PostItNote();
                    idea.Id = changeEvent.ChangedIdeaId;
                    idea.Content = changeEvent.MetaData;
                    UpdateContentEventExtractedHandler(idea);
                }
            }
        }
        void ExtractColorChangeEvent(TimelineChange change)
        {
            if (ColorChangeEventExtractedHandler != null)
            {
                ColorChangeEventExtractedHandler(change.ChangedIdeaId, (string)change.MetaData);
            }
        }
    }
}
