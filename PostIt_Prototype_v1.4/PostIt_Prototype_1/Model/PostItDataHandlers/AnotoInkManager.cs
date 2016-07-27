using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class AnotoInkManager : IPostItNetworkDataHandler
    {
        public delegate void InkTraceExtracted(AnotoInkTrace trace);
        private List<AnotoInkTrace> _inkTraces = null;
        byte[] _buffer = null;
        public event InkTraceExtracted TraceExtractedEventHandler = null;
        public AnotoInkManager()
        {
            _inkTraces = new List<AnotoInkTrace>();
        }
        public void networkReceivedDataHandler(byte[] data)
        {
            if (!AnotoInkTrace.CanExtractTraceFromBytes(data))
            {
                return;
            }
            var trace = new AnotoInkTrace();
            trace.ExtractDataFromFormatedBytes(data);
            _inkTraces.Add(trace);
            if (TraceExtractedEventHandler != null)
            {
                TraceExtractedEventHandler(trace);
            }
        }
        List<byte[]> SplitBytesToChunksByTrace(byte[] data)
        {
            var chunkList = new List<byte[]>();
            var tempData = (byte[])data.Clone();
            var dataStr = Encoding.UTF8.GetString(tempData);
            while (dataStr.Contains(Encoding.UTF8.GetString(AnotoInkTrace.PreTag))
                && dataStr.Contains(Encoding.UTF8.GetString(AnotoInkTrace.PosTag)))
            {
                var start = dataStr.IndexOf(Encoding.UTF8.GetString(AnotoInkTrace.PreTag));
                var end = dataStr.IndexOf(Encoding.UTF8.GetString(AnotoInkTrace.PosTag));
                var chunk = new byte[end - (start + AnotoInkTrace.PreTag.Length)];
                Array.Copy(tempData, start + AnotoInkTrace.PreTag.Length, chunk, 0, chunk.Length);
                chunkList.Add(chunk);                
            }
            return chunkList;
        }
        public bool IsAnotoMessage(byte[] msg)
        {
            return AnotoInkTrace.IsAnotoTraceMessage(Encoding.UTF8.GetString(msg));
        }
    }
}
