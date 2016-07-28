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
        
        List<GenericIdeationObjects.IdeationUnit> discardedIdeas;
        public event DiscardedIdeaReceived discardedIdeaReceivedEventHandler = null;
        public event DiscardedIdeaRestored discardedIdeaRestoredEventHandler = null;
        public event NewDiscardedIdeaListUpdated discardedIdeaListUpdatedHandler = null;
        public RecycleBinManager()
        {
            discardedIdeas = new List<GenericIdeationObjects.IdeationUnit>();
        }
        public GenericIdeationObjects.IdeationUnit getIdeaById(int id)
        {
            GenericIdeationObjects.IdeationUnit idea = null;
            for (var i = 0; i < discardedIdeas.Count; i++)
            {
                if (discardedIdeas[i].Id == id)
                {
                    idea = discardedIdeas[i];
                    break;
                }
            }
            return idea;
        }
        public void ReceiveDiscardedIdea(GenericIdeationObjects.IdeationUnit toBeDiscarded)
        {
            discardedIdeas.Add(toBeDiscarded);
            if (discardedIdeaReceivedEventHandler != null)
            {
                discardedIdeaReceivedEventHandler(toBeDiscarded);
            }
        }
        public void ReceiveDiscardedIdeaInBackground(GenericIdeationObjects.IdeationUnit toBeDiscarded)
        {
            discardedIdeas.Add(toBeDiscarded);
        }
        public void RestoreIdea(GenericIdeationObjects.IdeationUnit toBeRestored)
        {
            var idea = getIdeaById(toBeRestored.Id);
            if (idea != null)
            {
                discardedIdeas.Remove(idea);
                if (discardedIdeaRestoredEventHandler != null)
                {
                    discardedIdeaRestoredEventHandler(toBeRestored);
                }
            }
        }
        public void RestoreIdeaInBackground(GenericIdeationObjects.IdeationUnit toBeRestored)
        {
            var idea = getIdeaById(toBeRestored.Id);
            if (idea != null)
            {
                discardedIdeas.Remove(idea);
            }
        }
        public void reset()
        {
            discardedIdeas.Clear();
        }
        public void NotifyNewDiscardIdeasList()
        {
            if (discardedIdeaListUpdatedHandler != null)
            {
                discardedIdeaListUpdatedHandler(this.discardedIdeas);
            }
        }
    }
}
