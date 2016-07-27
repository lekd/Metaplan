using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.Recycle_Bin
{
    public class RecycleBinManager
    {
        public delegate void DiscardedIdeaReceived(GenericIdeationObjects.IdeationUnit toBeDiscarded);
        public delegate void DiscardedIdeaRestored(GenericIdeationObjects.IdeationUnit toBeStored);
        public delegate void NewDiscardedIdeaListUpdated(List<GenericIdeationObjects.IdeationUnit> newDiscardedList);
        
        List<GenericIdeationObjects.IdeationUnit> _discardedIdeas;
        public event DiscardedIdeaReceived DiscardedIdeaReceivedEventHandler = null;
        public event DiscardedIdeaRestored DiscardedIdeaRestoredEventHandler = null;
        public event NewDiscardedIdeaListUpdated DiscardedIdeaListUpdatedHandler = null;
        public RecycleBinManager()
        {
            _discardedIdeas = new List<GenericIdeationObjects.IdeationUnit>();
        }
        public GenericIdeationObjects.IdeationUnit GetIdeaById(int id)
        {
            GenericIdeationObjects.IdeationUnit idea = null;
            for (var i = 0; i < _discardedIdeas.Count; i++)
            {
                if (_discardedIdeas[i].Id == id)
                {
                    idea = _discardedIdeas[i];
                    break;
                }
            }
            return idea;
        }
        public void ReceiveDiscardedIdea(GenericIdeationObjects.IdeationUnit toBeDiscarded)
        {
            _discardedIdeas.Add(toBeDiscarded);
            if (DiscardedIdeaReceivedEventHandler != null)
            {
                DiscardedIdeaReceivedEventHandler(toBeDiscarded);
            }
        }
        public void ReceiveDiscardedIdeaInBackground(GenericIdeationObjects.IdeationUnit toBeDiscarded)
        {
            _discardedIdeas.Add(toBeDiscarded);
        }
        public void RestoreIdea(GenericIdeationObjects.IdeationUnit toBeRestored)
        {
            var idea = GetIdeaById(toBeRestored.Id);
            if (idea != null)
            {
                _discardedIdeas.Remove(idea);
                if (DiscardedIdeaRestoredEventHandler != null)
                {
                    DiscardedIdeaRestoredEventHandler(toBeRestored);
                }
            }
        }
        public void RestoreIdeaInBackground(GenericIdeationObjects.IdeationUnit toBeRestored)
        {
            var idea = GetIdeaById(toBeRestored.Id);
            if (idea != null)
            {
                _discardedIdeas.Remove(idea);
            }
        }
        public void Reset()
        {
            _discardedIdeas.Clear();
        }
        public void NotifyNewDiscardIdeasList()
        {
            if (DiscardedIdeaListUpdatedHandler != null)
            {
                DiscardedIdeaListUpdatedHandler(this._discardedIdeas);
            }
        }
    }
}
