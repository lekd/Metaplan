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
        public delegate void IdeaUpdatedEvent(IdeationUnit updatedIdea);

        public event NewIdeaAddedEvent ideaAddedEventHandler = null;
        public event IdeaRemovedEvent ideaRemovedHandler = null;
        public event IdeaUpdatedEvent ideaUpdatedHandler = null;
        #endregion
        List<IdeationUnit> _ideas;
        List<IdeationUnitGroup> _ideaGroups;
        AnotoPostItManager _anotoNoteManager = null;

        public AnotoPostItManager AnotoNoteManager
        {
            get { return _anotoNoteManager; }
            set { _anotoNoteManager = value; }
        }

        GenericPostItNoteManager _genericNoteManager = null;

        public GenericPostItNoteManager GenericNoteManager
        {
            get { return _genericNoteManager; }
            set { _genericNoteManager = value; }
        }
        public PostItGeneralManager()
        {
            _ideas = new List<IdeationUnit>();
            _ideaGroups = new List<IdeationUnitGroup>();
            
            _anotoNoteManager = new AnotoPostItManager();
            _anotoNoteManager.newNoteAddedHandler +=new AnotoPostItManager.NewPostItAdded(AddIdea);
            _anotoNoteManager.noteRemovedHandler += new AnotoPostItManager.PostItRemoved(RemoveIdea);
            _anotoNoteManager.noteContentUpdatedHandler += new AnotoPostItManager.PostItContentUpdated(UpdateIdeaContent);

            _genericNoteManager = new GenericPostItNoteManager();
            _genericNoteManager.noteAddedEventHandler +=new GenericPostItNoteManager.NewPostItAdded(AddIdea);
            _genericNoteManager.noteRemovedEventHandler +=new GenericPostItNoteManager.PostItRemoved(RemoveIdea);
            _genericNoteManager.noteUpdatedEventHandler +=new GenericPostItNoteManager.PostItUpdated(UpdateIdeaContent);
        }
        public void AddIdea(IdeationUnit idea)
        {
            if (idea != null)
            {
                if (!_ideas.Contains(idea))
                {
                    _ideas.Add(idea);
                    if (idea is IdeationUnitGroup)
                    {
                        _ideaGroups.Add((IdeationUnitGroup)idea);
                    }
                }
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
                if (_ideas.Contains(idea))
                {
                    idea.IsAvailable = false;
                    if (ideaRemovedHandler != null)
                    {
                        ideaRemovedHandler(idea);
                    }
                }
            }
        }
        public void UpdateIdeaContent(IdeationUnit idea)
        {
            if (ideaUpdatedHandler != null)
            {
                ideaUpdatedHandler(idea);
            }
        }
    }
}
