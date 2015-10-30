using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenericIdeationObjects;

namespace PostIt_Prototype_1.PostItBrainstorming
{
    public class PostItGeneralManager
    {
        #region Custom events
        public delegate void NewIdeaAddedEvent(IdeationUnit addedIdea);
        public delegate void IdeaRemovedEvent(IdeationUnit removedIdea);
        public delegate void IdeaUpdatedEvent(IdeationUnit updatedIdea, IdeationUnit.IdeaUpdateType updateType);
        public delegate void IdeaCollectionRollBackFinished(List<IdeationUnit> currentIdeas);

        public event NewIdeaAddedEvent ideaAddedEventHandler = null;
        public event IdeaRemovedEvent ideaRemovedHandler = null;
        public event IdeaUpdatedEvent ideaUpdatedHandler = null;
        public event IdeaCollectionRollBackFinished ideaCollectionRollBackFinishedEventHandler = null;
        #endregion
        List<IdeationUnit> _ideas;
       //GenericPostItNoteManager _genericNoteManager = null;

        
        public PostItGeneralManager()
        {
            _ideas = new List<IdeationUnit>();
            

            //_genericNoteManager = new GenericPostItNoteManager();
            //_genericNoteManager.noteAddedEventHandler +=new GenericPostItNoteManager.NewPostItAdded(AddIdea);
            //_genericNoteManager.noteRemovedEventHandler +=new GenericPostItNoteManager.PostItRemoved(RemoveIdea);
            //_genericNoteManager.noteUpdatedEventHandler +=new GenericPostItNoteManager.PostItUpdated(UpdateIdeaContent);
        }
        bool ContainIdea(IdeationUnit idea)
        {
            for (int i = 0; i < _ideas.Count; i++)
            {
                if (_ideas[i].Id == idea.Id)
                {
                    return true;
                }
            }
            return false;
        }
        IdeationUnit getIdeaWithId(int id)
        {
            for (int i = 0; i < _ideas.Count; i++)
            {
                if (_ideas[i].Id == id)
                {
                    return _ideas[i];
                }
            }
            return null;
        }
        public void HandleComingIdea(IdeationUnit idea)
        {
            if (!ContainIdea(idea))
            {
                AddIdea(idea);
            }
            else
            {
                restoreIdea(idea);
                UpdateIdeaContent(idea);
            }
        }
        public void AddIdea(IdeationUnit idea)
        {
            if (idea != null)
            {
                _ideas.Add(idea);
                if (ideaAddedEventHandler != null)
                {
                    ideaAddedEventHandler(idea);
                }
            }
        }
        public void RemoveIdea(IdeationUnit idea)
        {
            if (idea != null)
            {
                if (ContainIdea(idea))
                {
                    IdeationUnit existingIdea = getIdeaWithId(idea.Id);
                    existingIdea.IsAvailable = false;
                    if (ideaRemovedHandler != null)
                    {
                        ideaRemovedHandler(existingIdea);
                    }
                }
            }
        }

        public void UpdateIdeaContent(IdeationUnit idea)
        {
            if (ideaUpdatedHandler != null)
            {
                IdeationUnit existingIdea = getIdeaWithId(idea.Id);
                existingIdea.Content = idea.Content;
                if (ideaUpdatedHandler != null)
                {
                    ideaUpdatedHandler(existingIdea, IdeationUnit.IdeaUpdateType.Content);
                }
            }
        }
        public void UpdateIdeaPosition(int ideaID, float newX, float newY)
        {
            if (ideaUpdatedHandler != null)
            {
                IdeationUnit existingIdea = getIdeaWithId(ideaID);
                double distance = Utilities.UtilitiesLib.distanceBetweenTwoPoints(existingIdea.CenterX, existingIdea.CenterY, newX, newY);
                if (distance >= 50)
                {
                    existingIdea.CenterX = newX;
                    existingIdea.CenterY = newY;
                    if (ideaUpdatedHandler != null)
                    {
                        ideaUpdatedHandler(existingIdea, IdeationUnit.IdeaUpdateType.Position);
                    }
                }
            }
        }
        public void restoreIdea(IdeationUnit idea)
        {
            if (ContainIdea(idea))
            {
                IdeationUnit existingIdea = getIdeaWithId(idea.Id);
                existingIdea.IsAvailable = true;
            }
        }
        #region run-in-background methods
        public void reset()
        {
            _ideas.Clear();
        }
        public void AddIdeaInBackground(IdeationUnit idea)
        {
            if (idea != null)
            {
                _ideas.Add(idea);
            }
        }
        public void RemoveIdeaInBackground(IdeationUnit idea)
        {
            if (idea != null)
            {
                if (ContainIdea(idea))
                {
                    IdeationUnit existingIdea = getIdeaWithId(idea.Id);
                    existingIdea.IsAvailable = false;
                }
            }
        }
        public void UpdateIdeaContentInBackground(IdeationUnit idea)
        {
            if (ideaUpdatedHandler != null)
            {
                IdeationUnit existingIdea = getIdeaWithId(idea.Id);
                existingIdea.Content = idea.Content;
            }
        }
        public void UpdateIdeaPositionInBackground(int ideaID, float newX, float newY)
        {
            if (ideaUpdatedHandler != null)
            {
                IdeationUnit existingIdea = getIdeaWithId(ideaID);
                double distance = Utilities.UtilitiesLib.distanceBetweenTwoPoints(existingIdea.CenterX, existingIdea.CenterY, newX, newY);
                existingIdea.CenterX = newX;
                existingIdea.CenterY = newY;
            }
        }
        public void notifyIdeaCollectionRollBack()
        {
            if (ideaCollectionRollBackFinishedEventHandler != null)
            {
                ideaCollectionRollBackFinishedEventHandler(_ideas);
            }
        }
        #endregion
    }
}
