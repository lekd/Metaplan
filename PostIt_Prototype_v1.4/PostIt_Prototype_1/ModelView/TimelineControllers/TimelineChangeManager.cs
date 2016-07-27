using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;

namespace PostIt_Prototype_1.TimelineControllers
{
    public class TimelineChangeManager
    {
        public delegate void NewTimelineFrameAdded(TimelineFrame addedFrame);
        public delegate void StartEnumeratingFromBeginning();
        public delegate void FinishEnumeratingToTheSelected();

        TimelineFrame _currentFrame;
        List<TimelineFrame> _frames;

        //timeline storage
        TimelineStorage _timelineStorage = new TimelineStorage();
        //bridge between timeline and brainstorming manager
        TimlineEventIntepreter _eventIntepreter;

        public TimlineEventIntepreter EventIntepreter
        {
            get { return _eventIntepreter; }
            set { _eventIntepreter = value; }
        }
        public event NewTimelineFrameAdded NewTimelineFrameAddedEventHandler = null;
        public event StartEnumeratingFromBeginning StartEnumeratingEventHandler = null;
        public event FinishEnumeratingToTheSelected FinishEnumeratingEventHandler = null;

        public TimelineChangeManager()
        {
            _currentFrame = null;
            _frames = new List<TimelineFrame>();

            _timelineStorage.Initiate();
        }
        int GetNextFrameId()
        {
            if (_frames.Count == 0)
            {
                return 0;
            }
            return (_frames[_frames.Count - 1].Id + 1);
        }
        void AddFrame(TimelineFrame frame)
        {
            _frames.Add(frame);
            _currentFrame = frame;
            _timelineStorage.SaveFrame(frame);
            if (NewTimelineFrameAddedEventHandler != null)
            {
                NewTimelineFrameAddedEventHandler(frame);
            }
        }
        public void AddAddChange(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            if (ShouldAddDuplicatEframe())
            {
                AddDuplicateChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.Add,addedIdea.Id,addedIdea.Content);
            var frame = new TimelineFrame();
            frame.Id = GetNextFrameId();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddDeleteChange(int deletedIdeaId)
        {
            if (ShouldAddDuplicatEframe())
            {
                AddDuplicateChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.Delete, deletedIdeaId, null);
            var frame = new TimelineFrame();
            frame.Id = GetNextFrameId();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddRestoreChange(int restoredIdeaId)
        {
            if (ShouldAddDuplicatEframe())
            {
                AddDuplicateChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.Restore, restoredIdeaId, null);
            var frame = new TimelineFrame();
            frame.Id = GetNextFrameId();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddUpdatePositionChange(int updaptedNoteId, double newX, double newY)
        {
            if (ShouldAddDuplicatEframe())
            {
                AddDuplicateChange(_currentFrame.Id);
            }
            var newCenter = new System.Windows.Point(newX, newY);
            var change = new TimelineChange(TypeOfChange.Update, updaptedNoteId, newCenter);
            var frame = new TimelineFrame();
            frame.Id = GetNextFrameId();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddUpdateContentChange(int updatedNoteId, object newContent)
        {
            if (ShouldAddDuplicatEframe())
            {
                AddDuplicateChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.Update, updatedNoteId, newContent);
            var frame = new TimelineFrame();
            frame.Id = GetNextFrameId();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddColorChange(int noteId, string colorCode)
        {
            if (ShouldAddDuplicatEframe())
            {
                AddDuplicateChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.Color, noteId, colorCode);
            var frame = new TimelineFrame();
            frame.Id = GetNextFrameId();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddDuplicateChange(int refFrameId)
        {
            var change = new TimelineChange(TypeOfChange.Duplicate, refFrameId, null);
            var frame = new TimelineFrame();
            frame.Id = GetNextFrameId();
            frame.Change = change;
            AddFrame(frame);
        }
        public bool ShouldAddDuplicatEframe()
        {
            if (_frames.Count == 0)
            {
                return false;
            }
            return (_currentFrame.Id != _frames[_frames.Count - 1].Id);
        }
        public void SelectFrame(int frameId)
        {
            var selectedFrame = _timelineStorage.RetrieveFrameFromStorage(frameId);
            if (selectedFrame.Id == _currentFrame.Id)
            {
                return;
            }
            if (StartEnumeratingEventHandler != null)
            {
                StartEnumeratingEventHandler();
            }
            foreach (var f in _frames)
            {
                if (f.Change.ChangeType == TypeOfChange.Duplicate)
                {
                    JumpToFrame(f.Change.ChangedIdeaId);
                }
                else
                {
                    if (_eventIntepreter != null)
                    {
                        _eventIntepreter.IntepretEvent(f.Change);
                    }
                }
                if (f.Id == selectedFrame.Id)
                {
                    break;
                }
            }
            if (FinishEnumeratingEventHandler != null)
            {
                FinishEnumeratingEventHandler();
            }
            _currentFrame = selectedFrame;
        }

        void JumpToFrame(int frameId)
        {
            var jumpTo = _timelineStorage.RetrieveFrameFromStorage(frameId);
            if (StartEnumeratingEventHandler != null)
            {
                StartEnumeratingEventHandler();
            }
            foreach (var f in _frames)
            {
                if (_eventIntepreter != null)
                {
                    _eventIntepreter.IntepretEvent(f.Change);
                }
                if (f.Id == jumpTo.Id)
                {
                    break;
                }
            }
        }
    }
}
