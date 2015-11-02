using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class AnotoInkTrace
    {
        public static byte[] preTag = new byte[] { (byte)'<', (byte)'t', (byte)'r', (byte)'a', (byte)'c', (byte)'e', (byte)'>' };
        public static byte[] posTag = new byte[] { (byte)'<', (byte)'/',(byte)'t', (byte)'r', (byte)'a', (byte)'c', (byte)'e', (byte)'>' };
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
        public void addInkDot(AnotoInkDot inkDot)
        {
            _inkDots.Add(inkDot);
        }
        public void extractDataFromRawBytes(byte[] rawBytes)
        {
            _inkDots.Clear();
            int chunkSize = AnotoInkDot.size();
            int offset = 0;
            byte[] chunk = new byte[chunkSize];
            while (offset < rawBytes.Length-chunkSize)
            {
                Array.Copy(rawBytes, offset, chunk, 0, chunkSize);
                AnotoInkDot inkDot = new AnotoInkDot();
                inkDot.parseFromRawBytes(chunk);
                _inkDots.Add(inkDot);
                offset += chunkSize;
            }
        }
        public static bool canExtractTraceFromBytes(byte[] formatedBytes)
        {
            string formatedStr = Encoding.UTF8.GetString(formatedBytes);
            if ((!formatedStr.Contains(Encoding.UTF8.GetString(preTag))) ||
                (!formatedStr.Contains(Encoding.UTF8.GetString(posTag))))
            {
                return false;
            }
            return true;
        }
        public static bool IsAnotoTraceMessage(string msgStr)
        {
            if (msgStr.Contains(Encoding.UTF8.GetString(preTag)) ||
                msgStr.Contains(Encoding.UTF8.GetString(posTag)))
            {
                return true;
            }
            return false;
        }
        public void extractDataFromFormatedBytes(byte[] formatedBytes)
        {
            byte[] rawData = new byte[formatedBytes.Length - preTag.Length - posTag.Length];
            Array.Copy(formatedBytes, preTag.Length, rawData, 0, rawData.Length);
            extractDataFromRawBytes(rawData);
        }
        //get total length by accumulating component euclidean distances between dots
        public double getAccumulativeLength()
        {
		    AnotoInkDot prevDot = _inkDots[0];
		    double totalLength = 0;
		    foreach(AnotoInkDot inkDot in _inkDots){
			    if(inkDot.PaperNoteID!=prevDot.PaperNoteID){
				    prevDot = inkDot;
				    continue;
			    }
			    double distance = Utilities.UtilitiesLib.distanceBetweenTwoPoints(prevDot.X, prevDot.Y,
																    inkDot.X, inkDot.Y);
			    totalLength += distance;
			    prevDot = inkDot;
		    }
		    return totalLength;
	    }
        //use for detecting remove crossing-line gesture
        public PointF getLeftEndPoint()
        {
            PointF leftEndPoint = new PointF();
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
        public PointF getRightEndPoint()
        {
            PointF rightEndPoint = new PointF();
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
        public bool isStraightLine()
        {
            PointF leftEndPoint = getLeftEndPoint();
            PointF rightEndPoint = getRightEndPoint();
            double euclDistance = Utilities.UtilitiesLib.distanceBetweenTwoPoints(leftEndPoint.X, leftEndPoint.Y,
                                                                    rightEndPoint.X, rightEndPoint.Y);
            double totalLength = getAccumulativeLength();
            double gap = Math.Abs(totalLength - euclDistance);
            //lengths of the 2 distances are almost the same
            if ((gap / euclDistance) < 0.3)
            {
                return true;
            }
            return false;
        }
        public bool isComplete()
        {
            return _inkDots.Count > 0;
        }
        public bool isMultiIDTrace()
        {
		    int prevID = _inkDots[0].PaperNoteID;
		    foreach(AnotoInkDot inkDot in _inkDots){
			    if(inkDot.PaperNoteID!=prevID){
				    return true;
			    }
		    }
		    return false;
	    }
        public bool isPotentialAssignGesture()
        {
            List<PairInteger> IDsCountList = new List<PairInteger>();
		    foreach(AnotoInkDot inkDot in _inkDots){
			    if(IDsCountList.Count==0){
				    IDsCountList.Add(new PairInteger(inkDot.PaperNoteID, 1));
				    continue;
			    }
			    if(IDsCountList[IDsCountList.Count-1].Element_1!=inkDot.PaperNoteID){
				    IDsCountList.Add(new PairInteger(inkDot.PaperNoteID, 1));
			    }
			    else{
				    PairInteger lastIDCount = IDsCountList[IDsCountList.Count-1];
				    lastIDCount.Element_2++;
			    }
		    }
		    if(IDsCountList.Count!=3){
			    return false;
		    }
		    if(IDsCountList[0].Element_1!=IDsCountList[2].Element_1){
			    return false;
		    }
		    int seq0 = IDsCountList[0].Element_2;
		    int seq1 = IDsCountList[1].Element_2;
		    int seq2 = IDsCountList[2].Element_2;
		    int seqRat_1 = seq0>seq1?seq0/seq1:seq1/seq0;
		    int seqRat_2 = seq1>seq2?seq1/seq2:seq2/seq1;
		    int seqRat_3 = seq0>seq2?seq0/seq2:seq2/seq0;
		    if(seqRat_1>3 || seqRat_2>3 || seqRat_3>2){
			    return false;
		    }
            return true;
        }
        public List<AnotoInkTrace> splitToSingleIDTraces()
        {
		    List<AnotoInkTrace> singleIDTraces = new List<AnotoInkTrace>();
		    AnotoInkTrace curTrace = new AnotoInkTrace();
		    AnotoInkDot prevDot = _inkDots[0];
		    foreach(AnotoInkDot inkDot in _inkDots){
			    if(inkDot.PaperNoteID==prevDot.PaperNoteID){
				    curTrace.addInkDot(inkDot);
			    }
			    else{
				    singleIDTraces.Add(curTrace);
				    curTrace = new AnotoInkTrace();
				    curTrace.addInkDot(inkDot);
			    }
			    prevDot = inkDot;
		    }
		    return singleIDTraces;
	    }

        class PairInteger
        {
            public PairInteger(int elm1, int elm2)
            {
                element_1 = elm1;
                element_2 = elm2;
            }
            int element_1;

            public int Element_1
            {
                get { return element_1; }
                set { element_1 = value; }
            }
            int element_2;

            public int Element_2
            {
                get { return element_2; }
                set { element_2 = value; }
            }
        }
    }
}
