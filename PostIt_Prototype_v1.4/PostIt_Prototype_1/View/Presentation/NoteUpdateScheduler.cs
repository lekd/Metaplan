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
        DateTime _lastCallTime;
        Timer _timer;
        public event UpdateIntervalTickedEvent UpdateEventHandler = null;
        bool _isRunning = true;
        public NoteUpdateScheduler()
        {
            _lastCallTime = DateTime.Now;
            _timer = new Timer(CheckTimeToUpdate, null, TimeSpan.FromSeconds(Properties.Settings.Default.DelayInitialCloudPoll), TimeSpan.FromSeconds(Properties.Settings.Default.CloudUpdateInterval));
            _isRunning = true;
        }
        private void CheckTimeToUpdate(object state)
        {
            if (!_isRunning)
            {
                return;
            }
            if (UpdateEventHandler != null)
            {
                UpdateEventHandler();
            }
        }
        public void Stop()
        {
            _isRunning = false;
        }
    }
}
