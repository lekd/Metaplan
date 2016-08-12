using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using WhiteboardApp.PostItDataHandlers;

namespace WhiteboardApp.PostItObjects
{
    public class AnotoPostIt:GenericIdeationObjects.IdeationUnit
    {
        #region Custom Events
        public delegate void DrawablePostItRemoved(AnotoPostIt removedNote);
        public delegate void DrawableContentUpdated(AnotoPostIt updatedNote);
        public event DrawablePostItRemoved postItRemovedHandler = null;
        public event DrawableContentUpdated contentUpdatedHandler = null;
        #endregion
        public AnotoPostIt()
        {
            initEmptyContent();
        }
        public AnotoPostIt(int id=0,float x=0,float y=0)
        {
            _id = id;
            _centerX = x;
            _centerY = y;
            initEmptyContent();
        }
        void initEmptyContent()
        {
            _content = new List<AnotoInkTrace>();
        }
        public override void updateContent(Object update)
        {
            var traces = (List<AnotoInkTrace>)_content;
            traces.Add((AnotoInkTrace)update);
            if (!_isAvailable)
            {
                return;
            }
            if (contentUpdatedHandler != null)
            {
                contentUpdatedHandler(this);
            }
            if (containRemoveGesture())
            {
                if (postItRemovedHandler != null)
                {
                    postItRemovedHandler(this);
                }
            }
        }
        private bool containRemoveGesture()
        {
            //remove gesture is 2 crossing lines
            // each goes from a corner to the opposite one of the note
            //just check the two latest note
            var traces = (List<AnotoInkTrace>)_content;
            if (traces.Count < 2)
            {
                return false;
            }
            var trace_1 = traces[traces.Count - 1];
            /*if(!trace_1.isInNoteTrace()){
                return false;
            }*/
            var trace_2 = traces[traces.Count - 2];
            /*if(!trace_2.isInNoteTrace()){
                return false;
            }*/
            if (!trace_1.isStraightLine())
            {
                return false;
            }
            if (!trace_2.isStraightLine())
            {
                return false;
            }
            //if both two lines do not start near (0,0)
            // then this should not be remove gestures

            if (trace_1.getLeftEndPoint().X > 50
                && trace_2.getLeftEndPoint().X > 50
                && trace_1.getLeftEndPoint().Y > 50
                && trace_2.getLeftEndPoint().Y > 50)
            {
                return false;
            }
            AnotoInkTrace updownTrace = null;
            updownTrace = trace_1.getLeftEndPoint().Y < trace_2.getLeftEndPoint().Y ? trace_1 : trace_2;
            AnotoInkTrace downupTrace = null;
            downupTrace = trace_1.getLeftEndPoint().Y > trace_2.getLeftEndPoint().Y ? trace_1 : trace_2;
            var leftDif = Math.Abs(updownTrace.getLeftEndPoint().X - downupTrace.getLeftEndPoint().X);
            var topDif = Math.Abs(updownTrace.getLeftEndPoint().Y - downupTrace.getRightEndPoint().Y);
            var rightDif = Math.Abs(updownTrace.getRightEndPoint().X - downupTrace.getRightEndPoint().X);
            var bottomDif = Math.Abs(updownTrace.getRightEndPoint().Y - downupTrace.getLeftEndPoint().Y);
            var MAX_DIF = 50;
            if (leftDif > MAX_DIF || topDif > MAX_DIF || rightDif > MAX_DIF || bottomDif > MAX_DIF)
            {
                return false;
            }
            return true;
        }
        public void extractPositionFromAssigningTrace(AnotoInkTrace trace)
        {
            var outBorder1 = new PointF();
            var inBorder1 = new PointF();
            var inBorder2 = new PointF();
            var outBorder2 = new PointF();
            var prevDot = trace.InkDots[0];
            var curIndex = 0;
            for (; curIndex < trace.InkDots.Count; curIndex++)
            {
                var curDot = trace.InkDots[curIndex];
                if (curDot.PaperNoteID != prevDot.PaperNoteID)
                {
                    //move from main canvas to current PostIt
                    if (curDot.PaperNoteID == this.Id)
                    {
                        //then the point right outside the PostIt is prevDot
                        outBorder1.X = prevDot.X;
                        outBorder1.Y = prevDot.Y;
                        //the first point right inside the PostIt is curDot
                        inBorder1.X = curDot.X;
                        inBorder1.Y = curDot.Y;
                    }
                    else
                    {
                        //move from current PostIt back to the main canvas
                        //then prevDot is the last point inside the PostIt
                        inBorder2.X = prevDot.X;
                        inBorder2.Y = prevDot.Y;
                        //the next dot (curDot) lies outside
                        outBorder2.X = curDot.X;
                        outBorder2.Y = curDot.Y;
                    }
                }
                prevDot = curDot;
            }
            //swapping if trace drawn down-to-top
            if (outBorder2.X < outBorder1.X ||
                    outBorder2.Y < outBorder1.Y)
            {
                var temp = outBorder2;
                outBorder2 = outBorder1;
                outBorder1 = temp;

                temp = inBorder2;
                inBorder2 = inBorder1;
                inBorder1 = temp;
            }
            //interpolate PostIt's position on the main canvas
            //assume outBorder1 and inBorder are very close
            //there is no much difference between two points
            _centerX = outBorder1.X - inBorder1.X;
            _centerY = outBorder1.Y - inBorder1.Y;
        }
    }
}
