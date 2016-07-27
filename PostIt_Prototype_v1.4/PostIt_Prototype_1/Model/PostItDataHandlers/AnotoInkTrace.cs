using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class AnotoInkTrace
    {
        public static byte[] PreTag = new byte[] { (byte)'<', (byte)'t', (byte)'r', (byte)'a', (byte)'c', (byte)'e', (byte)'>' };
        public static byte[] PosTag = new byte[] { (byte)'<', (byte)'/',(byte)'t', (byte)'r', (byte)'a', (byte)'c', (byte)'e', (byte)'>' };
        List<AnotoInkDot> _inkDots = null;

        public List<AnotoInkDot> InkDots
        {
            get { return _inkDots; }
            set { _inkDots = value; }
        }
        public AnotoInkTrace()
        {
            _inkDots = new List<AnotoInkDot>();
        }
        public void AddInkDot(AnotoInkDot inkDot)
        {
            _inkDots.Add(inkDot);
        }
        public void ExtractDataFromRawBytes(byte[] rawBytes)
        {
            _inkDots.Clear();
            var chunkSize = AnotoInkDot.Size();
            var offset = 0;
            var chunk = new byte[chunkSize];
            while (offset < rawBytes.Length-chunkSize)
            {
                Array.Copy(rawBytes, offset, chunk, 0, chunkSize);
                var inkDot = new AnotoInkDot();
                inkDot.ParseFromRawBytes(chunk);
                _inkDots.Add(inkDot);
                offset += chunkSize;
            }
        }
        public static bool CanExtractTraceFromBytes(byte[] formatedBytes)
        {
            var formatedStr = Encoding.UTF8.GetString(formatedBytes);
            if ((!formatedStr.Contains(Encoding.UTF8.GetString(PreTag))) ||
                (!formatedStr.Contains(Encoding.UTF8.GetString(PosTag))))
            {
                return false;
            }
            return true;
        }
        public static bool IsAnotoTraceMessage(string msgStr)
        {
            if (msgStr.Contains(Encoding.UTF8.GetString(PreTag)) ||
                msgStr.Contains(Encoding.UTF8.GetString(PosTag)))
            {
                return true;
            }
            return false;
        }
        public void ExtractDataFromFormatedBytes(byte[] formatedBytes)
        {
            var rawData = new byte[formatedBytes.Length - PreTag.Length - PosTag.Length];
            Array.Copy(formatedBytes, PreTag.Length, rawData, 0, rawData.Length);
            ExtractDataFromRawBytes(rawData);
        }
        //get total length by accumulating component euclidean distances between dots
        public double GetAccumulativeLength()
        {
		    var prevDot = _inkDots[0];
		    double totalLength = 0;
		    foreach(var inkDot in _inkDots){
			    if(inkDot.PaperNoteId!=prevDot.PaperNoteId){
				    prevDot = inkDot;
				    continue;
			    }
			    var distance = Utilities.UtilitiesLib.DistanceBetweenTwoPoints(prevDot.X, prevDot.Y,
																    inkDot.X, inkDot.Y);
			    totalLength += distance;
			    prevDot = inkDot;
		    }
		    return totalLength;
	    }
        //use for detecting remove crossing-line gesture
        public PointF GetLeftEndPoint()
        {
            var leftEndPoint = new PointF();
            if (_inkDots[0].X < _inkDots[_inkDots.Count - 1].X)
            {
                leftEndPoint.X = _inkDots[0].X;
                leftEndPoint.Y = _inkDots[0].Y;
            }
            else
            {
                leftEndPoint.X = _inkDots[_inkDots.Count - 1].X;
                leftEndPoint.Y = _inkDots[_inkDots.Count - 1].Y;
            }
            return leftEndPoint;
        }
        public PointF GetRightEndPoint()
        {
            var rightEndPoint = new PointF();
            if (_inkDots[0].X > _inkDots[_inkDots.Count - 1].X)
            {
                rightEndPoint.X = _inkDots[0].X;
                rightEndPoint.Y = _inkDots[0].Y;
            }
            else
            {
                rightEndPoint.X = _inkDots[_inkDots.Count - 1].X;
                rightEndPoint.Y = _inkDots[_inkDots.Count - 1].Y;
            }
            return rightEndPoint;
        }
        //just applied if trace within a note
        public bool IsStraightLine()
        {
            var leftEndPoint = GetLeftEndPoint();
            var rightEndPoint = GetRightEndPoint();
            var euclDistance = Utilities.UtilitiesLib.DistanceBetweenTwoPoints(leftEndPoint.X, leftEndPoint.Y,
                                                                    rightEndPoint.X, rightEndPoint.Y);
            var totalLength = GetAccumulativeLength();
            var gap = Math.Abs(totalLength - euclDistance);
            //lengths of the 2 distances are almost the same
            if ((gap / euclDistance) < 0.3)
            {
                return true;
            }
            return false;
        }
        public bool IsComplete()
        {
            return _inkDots.Count > 0;
        }
        public bool IsMultiIdTrace()
        {
		    var prevId = _inkDots[0].PaperNoteId;
		    foreach(var inkDot in _inkDots){
			    if(inkDot.PaperNoteId!=prevId){
				    return true;
			    }
		    }
		    return false;
	    }
        public bool IsPotentialAssignGesture()
        {
            var dsCountList = new List<PairInteger>();
		    foreach(var inkDot in _inkDots){
			    if(dsCountList.Count==0){
				    dsCountList.Add(new PairInteger(inkDot.PaperNoteId, 1));
				    continue;
			    }
			    if(dsCountList[dsCountList.Count-1].Element1!=inkDot.PaperNoteId){
				    dsCountList.Add(new PairInteger(inkDot.PaperNoteId, 1));
			    }
			    else{
				    var lastIdCount = dsCountList[dsCountList.Count-1];
				    lastIdCount.Element2++;
			    }
		    }
		    if(dsCountList.Count!=3){
			    return false;
		    }
		    if(dsCountList[0].Element1!=dsCountList[2].Element1){
			    return false;
		    }
		    var seq0 = dsCountList[0].Element2;
		    var seq1 = dsCountList[1].Element2;
		    var seq2 = dsCountList[2].Element2;
		    var seqRat1 = seq0>seq1?seq0/seq1:seq1/seq0;
		    var seqRat2 = seq1>seq2?seq1/seq2:seq2/seq1;
		    var seqRat3 = seq0>seq2?seq0/seq2:seq2/seq0;
		    if(seqRat1>3 || seqRat2>3 || seqRat3>2){
			    return false;
		    }
            return true;
        }
        public List<AnotoInkTrace> SplitToSingleIdTraces()
        {
		    var singleIdTraces = new List<AnotoInkTrace>();
		    var curTrace = new AnotoInkTrace();
		    var prevDot = _inkDots[0];
		    foreach(var inkDot in _inkDots){
			    if(inkDot.PaperNoteId==prevDot.PaperNoteId){
				    curTrace.AddInkDot(inkDot);
			    }
			    else{
				    singleIdTraces.Add(curTrace);
				    curTrace = new AnotoInkTrace();
				    curTrace.AddInkDot(inkDot);
			    }
			    prevDot = inkDot;
		    }
		    return singleIdTraces;
	    }

        class PairInteger
        {
            public PairInteger(int elm1, int elm2)
            {
                _element1 = elm1;
                _element2 = elm2;
            }
            int _element1;

            public int Element1
            {
                get { return _element1; }
                set { _element1 = value; }
            }
            int _element2;

            public int Element2
            {
                get { return _element2; }
                set { _element2 = value; }
            }
        }
    }
}
