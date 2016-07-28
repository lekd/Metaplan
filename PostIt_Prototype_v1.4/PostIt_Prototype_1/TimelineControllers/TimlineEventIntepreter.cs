using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.TimelineControllers
{
    public class TimlineEventIntepreter
    {
        public delegate void ADDIdeaCommandExtracted(GenericIdeationObjects.IdeationUnit idea);
        public delegate void REMOVEIdeaCommandExtracted(GenericIdeationObjects.IdeationUnit idea);
        public delegate void RESTOREIdeaCommandExtraced(GenericIdeationObjects.IdeationUnit idea);
        public delegate void UPDATEIdeaPositionCommandExtracted(int ideaID, float newX, float newY);
        public delegate void UPDATEIdeaContentCommandExtracted(GenericIdeationObjects.IdeationUnit idea);
        public delegate void COLORChangeCommandExtracted(int ideaID, string colorCode);

        public event ADDIdeaCommandExtracted ADDeventExtractedHandler = null;
        public event REMOVEIdeaCommandExtracted REMOVEeventExtractedHandler = null;
        public event RESTOREIdeaCommandExtraced RESTOREeventExtractedHandler = null;
        public event UPDATEIdeaPositionCommandExtracted UPDATEPosEventExtractedHandler = null;
        public event UPDATEIdeaContentCommandExtracted UPDATEContentEventExtractedHandler = null;
        public event COLORChangeCommandExtracted COLORChangeEventExtractedHandler = null;
        public void IntepretEvent(TimelineChange changeEvent)
        {
            switch (changeEvent.ChangeType)
            {
                case TypeOfChange.ADD:
                    ExtractADDevent(changeEvent);
                    break;
                case TypeOfChange.DELETE:
                    ExtractREMOVEevent(changeEvent);
                    break;
                case TypeOfChange.RESTORE:
                    ExtractRESTOREevent(changeEvent);
                    break;
                case TypeOfChange.UPDATE:
                    ExtractUPDATEevent(changeEvent);
                    break;
                case TypeOfChange.COLOR:
                    ExtractCOLORChangeEvent(changeEvent);
                    break;
            }
        }
        void ExtractADDevent(TimelineChange changeEvent)
        {
            GenericIdeationObjects.IdeationUnit idea = new PostItObjects.PostItNote();
            //this is a short-term solution, in the future need to re-implemented more sustainably
            if (changeEvent.MetaData is StrokeData)//this is the addition of a stroke
            {
                idea = new PostItObjects.StrokeBasedIdea();
            }
            idea.Id = changeEvent.ChangedIdeaID;
            idea.CenterX = 0;
            idea.CenterY = 0;
            idea.Content = changeEvent.MetaData;
            if (ADDeventExtractedHandler != null)
            {
                ADDeventExtractedHandler(idea);
            }
        }
        void ExtractREMOVEevent(TimelineChange changeEvent)
        {
            GenericIdeationObjects.IdeationUnit idea = new PostItObjects.PostItNote();
            idea.Id = changeEvent.ChangedIdeaID;
            if (REMOVEeventExtractedHandler != null)
            {
                REMOVEeventExtractedHandler(idea);
            }
        }
        void ExtractRESTOREevent(TimelineChange changeEvent)
        {
            GenericIdeationObjects.IdeationUnit idea = new PostItObjects.PostItNote();
            idea.Id = changeEvent.ChangedIdeaID;
            if (REMOVEeventExtractedHandler != null)
            {
                RESTOREeventExtractedHandler(idea);
            }
        }
        void ExtractUPDATEevent(TimelineChange changeEvent)
        {
            if (changeEvent.MetaData is Point)
            {
                if (UPDATEPosEventExtractedHandler != null)
                {
                    var newPos = (Point)changeEvent.MetaData;
                    UPDATEPosEventExtractedHandler(changeEvent.ChangedIdeaID, (float)newPos.X, (float)newPos.Y);
                }
            }
            else
            {
                if (UPDATEContentEventExtractedHandler != null)
                {
                    GenericIdeationObjects.IdeationUnit idea = new PostItObjects.PostItNote();
                    idea.Id = changeEvent.ChangedIdeaID;
                    idea.Content = changeEvent.MetaData;
                    UPDATEContentEventExtractedHandler(idea);
                }
            }
        }
        void ExtractCOLORChangeEvent(TimelineChange change)
        {
            if (COLORChangeEventExtractedHandler != null)
            {
                COLORChangeEventExtractedHandler(change.ChangedIdeaID, (string)change.MetaData);
            }
        }
    }
}
