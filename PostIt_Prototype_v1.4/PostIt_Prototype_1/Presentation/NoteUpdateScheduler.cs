using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace WhiteboardApp.Presentation
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
            _timer = new Timer(CheckTimeToUpdate, null, TimeSpan.FromSeconds(Properties.Settings.Default.DelayInitialCloudPoll), TimeSpan.FromSeconds(Properties.Settings.Default.CloudUpdateInterval));
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
