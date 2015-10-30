using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace PostIt_Prototype_1.Presentation
{
    public class NoteUpdateScheduler
    {
        public delegate void UpdateIntervalTickedEvent();
        DateTime lastCallTime;
        Timer _timer;
        public event UpdateIntervalTickedEvent updateEventHandler = null;
        bool isRunning = true;
        public NoteUpdateScheduler()
        {
            lastCallTime = DateTime.Now;
            _timer = new Timer(CheckTimeToUpdate, null, TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(10));
            isRunning = true;
        }
        private void CheckTimeToUpdate(object state)
        {
            if (!isRunning)
            {
                return;
            }
            if (updateEventHandler != null)
            {
                updateEventHandler();
            }
        }
        public void Stop()
        {
            isRunning = false;
        }
    }
}
