using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GenericIdeationObjects;
using System.Diagnostics;
using PostIt_Prototype_1.ModelView.PostItObjects;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.PostItBrainstorming
{
    public class PostItGeneralManager
    {
        #region Custom events
        public delegate void NewIdeaAddedEvent(IdeationUnit addedIdea);
        public delegate void IdeaRemovedEvent(IdeationUnit removedIdea);
        public delegate void IdeaRestoredEvent(IdeationUnit restoredIdea);
        public delegate void IdeaUpdatedEvent(IdeationUnit updatedIdea, IdeationUnit.IdeaUpdateType updateType);
        public delegate void IdeaUiColorChangeEvent(IdeationUnit updatedIdea, string colorCode);
        public delegate void IdeaCollectionRollBackFinished(List<IdeationUnit> currentIdeas);

        public event NewIdeaAddedEvent IdeaAddedEventHandler = null;
        public event IdeaRemovedEvent IdeaRemovedHandler = null;
        public event IdeaRestoredEvent IdeaRestoredHandler = null;
        public event IdeaUpdatedEvent IdeaUpdatedHandler = null;
        public event IdeaUiColorChangeEvent IdeaUiColorChangeHandler = null;
        public event IdeaCollectionRollBackFinished IdeaCollectionRollBackFinishedEventHandler = null;
        #endregion
        List<IdeationUnit> _ideas;

        public List<IdeationUnit> Ideas
        {
            get { return _ideas; }
            set { _ideas = value; }
        }
        Recycle_Bin.RecycleBinManager _trashManager;

        public Recycle_Bin.RecycleBinManager TrashManager
        {
            get { return _trashManager; }
            set { _trashManager = value; }
        }
       //GenericPostItNoteManager _genericNoteManager = null;

        
        public PostItGeneralManager()
        {
            _ideas = new List<IdeationUnit>();
            _trashManager = new Recycle_Bin.RecycleBinManager();
            _trashManager.DiscardedIdeaRestoredEventHandler +=new Recycle_Bin.RecycleBinManager.DiscardedIdeaRestored(this.RestoreIdea);

            //_genericNoteManager = new GenericPostItNoteManager();
            //_genericNoteManager.noteAddedEventHandler +=new GenericPostItNoteManager.NewPostItAdded(AddIdea);
            //_genericNoteManager.noteRemovedEventHandler +=new GenericPostItNoteManager.PostItRemoved(RemoveIdea);
            //_genericNoteManager.noteUpdatedEventHandler +=new GenericPostItNoteManager.PostItUpdated(UpdateIdeaContent);
        }
        bool ContainIdea(IdeationUnit idea)
        {
            for (var i = 0; i < _ideas.Count; i++)
            {
                if (_ideas[i].Id == idea.Id)
                {
                    return true;
                }
            }
            return false;
        }
        IdeationUnit GetIdeaWithId(int id)
        {
            for (var i = 0; i < _ideas.Count; i++)
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
                RestoreIdea(idea);
                UpdateIdeaContent(idea);
            }
        }
        private object _prevIdea;
        public void AddIdea(IdeationUnit idea)
        {            
            if (idea != null)
            {
                _ideas.Add(idea);
                if (IdeaAddedEventHandler != null)
                {
                    IdeaAddedEventHandler(idea);
                }
            }
            if (_prevIdea == idea)
                Debugger.Break();
            _prevIdea = idea;
        }
        public void RemoveIdea(IdeationUnit idea)
        {
            if (idea != null)
            {
                if (ContainIdea(idea))
                {
                    var existingIdea = GetIdeaWithId(idea.Id);
                    existingIdea.IsAvailable = false;
                    if (IdeaRemovedHandler != null)
                    {
                        IdeaRemovedHandler(existingIdea);
                    }
                    _trashManager.ReceiveDiscardedIdea(idea);
                }
            }
        }
        public void RestoreIdea(IdeationUnit idea)
        {
            if (ContainIdea(idea))
            {
                var existingIdea = GetIdeaWithId(idea.Id);
                if(!existingIdea.IsAvailable)
                {
                    existingIdea.IsAvailable = true;
                    if (IdeaRestoredHandler != null)
                    {
                        IdeaRestoredHandler(existingIdea);
                    }
                }
            }
        }
        public void UpdateIdeaContent(IdeationUnit idea)
        {
            if (IdeaUpdatedHandler != null)
            {
                var existingIdea = GetIdeaWithId(idea.Id);
                existingIdea.Content = idea.Content;
                if (IdeaUpdatedHandler != null)
                {
                    IdeaUpdatedHandler(existingIdea, IdeationUnit.IdeaUpdateType.Content);
                }
            }
        }
        public void UpdateIdeaPosition(int ideaId, float newX, float newY)
        {
            if (IdeaUpdatedHandler != null)
            {
                var existingIdea = GetIdeaWithId(ideaId);
                var distance = Utilities.UtilitiesLib.DistanceBetweenTwoPoints(existingIdea.CenterX, existingIdea.CenterY, newX, newY);
                if (distance >= Properties.Settings.Default.MinDistanceForTranslationEvent)
                {
                    existingIdea.CenterX = newX;
                    existingIdea.CenterY = newY;
                    if (IdeaUpdatedHandler != null)
                    {
                        IdeaUpdatedHandler(existingIdea, IdeationUnit.IdeaUpdateType.Position);
                    }
                }
            }
        }
        public void ChangeIdeaUiColor(int ideaId, string colorCode)
        {
            if (IdeaUiColorChangeHandler != null)
            {
                var existingIdea = GetIdeaWithId(ideaId);
                IdeaUiColorChangeHandler(existingIdea, colorCode);
            }
        }
        public void NotifyIdeaCollectionRollBack()
        {
            if (IdeaCollectionRollBackFinishedEventHandler != null)
            {
                IdeaCollectionRollBackFinishedEventHandler(_ideas);
                _trashManager.NotifyNewDiscardIdeasList();
            }
        }
        #region run-in-background methods
        public void Reset()
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
                    var existingIdea = GetIdeaWithId(idea.Id);
                    existingIdea.IsAvailable = false;
                    _trashManager.ReceiveDiscardedIdeaInBackground(idea);
                }
            }
        }
        public void UpdateIdeaContentInBackground(IdeationUnit idea)
        {
            if (IdeaUpdatedHandler != null)
            {
                var existingIdea = GetIdeaWithId(idea.Id);
                existingIdea.Content = idea.Content;
            }
        }
        public void UpdateIdeaPositionInBackground(int ideaId, float newX, float newY)
        {
            if (IdeaUpdatedHandler != null)
            {
                var existingIdea = GetIdeaWithId(ideaId);
                var distance = Utilities.UtilitiesLib.DistanceBetweenTwoPoints(existingIdea.CenterX, existingIdea.CenterY, newX, newY);
                existingIdea.CenterX = newX;
                existingIdea.CenterY = newY;
            }
        }
        public void RestoreIdeaInBackground(IdeationUnit idea)
        {
            if (ContainIdea(idea))
            {
                var existingIdea = GetIdeaWithId(idea.Id);
                existingIdea.IsAvailable = true;
                _trashManager.RestoreIdeaInBackground(idea);
            }
        }
        public void ChangeIdeaUiColorInBackground(int ideaId, string colorCode)
        {
            var postItIdea = (PostItNote)GetIdeaWithId(ideaId);
            postItIdea.MetaData.UiBackgroundColor = colorCode;
        }
        #endregion
    }
}
