﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Drawing;

namespace WhiteboardApp.TimelineControllers
{
    public class TimelineChangeManager
    {
        public delegate void NewTimelineFrameAdded(TimelineFrame addedFrame);
        public delegate void StartEnumeratingFromBeginning();
        public delegate void FinishEnumeratingToTheSelected();

        TimelineFrame _currentFrame;
        List<TimelineFrame> _frames;

        //timeline storage
        TimelineStorage timelineStorage = new TimelineStorage();
        //bridge between timeline and brainstorming manager
        TimlineEventIntepreter _eventIntepreter;

        public TimlineEventIntepreter EventIntepreter
        {
            get { return _eventIntepreter; }
            set { _eventIntepreter = value; }
        }
        public event NewTimelineFrameAdded newTimelineFrameAddedEventHandler = null;
        public event StartEnumeratingFromBeginning startEnumeratingEventHandler = null;
        public event FinishEnumeratingToTheSelected finishEnumeratingEventHandler = null;

        public TimelineChangeManager()
        {
            _currentFrame = null;
            _frames = new List<TimelineFrame>();

            timelineStorage.Initiate();
        }
        int getNextFrameID()
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
            timelineStorage.saveFrame(frame);
            if (newTimelineFrameAddedEventHandler != null)
            {
                newTimelineFrameAddedEventHandler(frame);
            }
        }
        public void AddADDChange(GenericIdeationObjects.IdeationUnit addedIdea)
        {
            if (shouldAddDUPLICATEframe())
            {
                AddDUPLICATEChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.ADD,addedIdea.Id,addedIdea.Content);
            var frame = new TimelineFrame();
            frame.Id = getNextFrameID();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddDELETEChange(int deletedIdeaID)
        {
            if (shouldAddDUPLICATEframe())
            {
                AddDUPLICATEChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.DELETE, deletedIdeaID, null);
            var frame = new TimelineFrame();
            frame.Id = getNextFrameID();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddRESTOREChange(int restoredIdeaID)
        {
            if (shouldAddDUPLICATEframe())
            {
                AddDUPLICATEChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.RESTORE, restoredIdeaID, null);
            var frame = new TimelineFrame();
            frame.Id = getNextFrameID();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddUPDATEPositionChange(int updaptedNoteID, double newX, double newY)
        {
            if (shouldAddDUPLICATEframe())
            {
                AddDUPLICATEChange(_currentFrame.Id);
            }
            var newCenter = new System.Windows.Point(newX, newY);
            var change = new TimelineChange(TypeOfChange.UPDATE, updaptedNoteID, newCenter);
            var frame = new TimelineFrame();
            frame.Id = getNextFrameID();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddUPDATEContentChange(int updatedNoteID, object newContent)
        {
            if (shouldAddDUPLICATEframe())
            {
                AddDUPLICATEChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.UPDATE, updatedNoteID, newContent);
            var frame = new TimelineFrame();
            frame.Id = getNextFrameID();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddCOLORChange(int noteID, string colorCode)
        {
            if (shouldAddDUPLICATEframe())
            {
                AddDUPLICATEChange(_currentFrame.Id);
            }
            var change = new TimelineChange(TypeOfChange.COLOR, noteID, colorCode);
            var frame = new TimelineFrame();
            frame.Id = getNextFrameID();
            frame.Change = change;
            AddFrame(frame);
        }
        public void AddDUPLICATEChange(int refFrameID)
        {
            var change = new TimelineChange(TypeOfChange.DUPLICATE, refFrameID, null);
            var frame = new TimelineFrame();
            frame.Id = getNextFrameID();
            frame.Change = change;
            AddFrame(frame);
        }
        public bool shouldAddDUPLICATEframe()
        {
            if (_frames.Count == 0)
            {
                return false;
            }
            return (_currentFrame.Id != _frames[_frames.Count - 1].Id);
        }
        public void SelectFrame(int frameID)
        {
            var selectedFrame = timelineStorage.retrieveFrameFromStorage(frameID);
            if (selectedFrame.Id == _currentFrame.Id)
            {
                return;
            }
            if (startEnumeratingEventHandler != null)
            {
                startEnumeratingEventHandler();
            }
            foreach (var f in _frames)
            {
                if (f.Change.ChangeType == TypeOfChange.DUPLICATE)
                {
                    JumpToFrame(f.Change.ChangedIdeaID);
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
            if (finishEnumeratingEventHandler != null)
            {
                finishEnumeratingEventHandler();
            }
            _currentFrame = selectedFrame;
        }

        void JumpToFrame(int frameID)
        {
            var jumpTo = timelineStorage.retrieveFrameFromStorage(frameID);
            if (startEnumeratingEventHandler != null)
            {
                startEnumeratingEventHandler();
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
