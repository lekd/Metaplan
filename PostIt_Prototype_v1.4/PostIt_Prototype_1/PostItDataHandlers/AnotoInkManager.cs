using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class AnotoInkManager : IPostItNetworkDataHandler
    {
        public delegate void InkTraceExtracted(AnotoInkTrace trace);
        private List<AnotoInkTrace> inkTraces = null;
        byte[] buffer = null;
        public event InkTraceExtracted traceExtractedEventHandler = null;
        public AnotoInkManager()
        {
            inkTraces = new List<AnotoInkTrace>();
        }
        public void networkReceivedDataHandler(byte[] data)
        {
            if (!AnotoInkTrace.canExtractTraceFromBytes(data))
            {
                return;
            }
            AnotoInkTrace trace = new AnotoInkTrace();
            trace.extractDataFromFormatedBytes(data);
            inkTraces.Add(trace);
            if (traceExtractedEventHandler != null)
            {
                traceExtractedEventHandler(trace);
            }
        }
        List<byte[]> splitBytesToChunksByTrace(byte[] data)
        {
            List<byte[]> chunkList = new List<byte[]>();
            byte[] tempData = (byte[])data.Clone();
            string dataStr = Encoding.UTF8.GetString(tempData);
            while (dataStr.Contains(Encoding.UTF8.GetString(AnotoInkTrace.preTag))
                && dataStr.Contains(Encoding.UTF8.GetString(AnotoInkTrace.posTag)))
            {
                int start = dataStr.IndexOf(Encoding.UTF8.GetString(AnotoInkTrace.preTag));
                int end = dataStr.IndexOf(Encoding.UTF8.GetString(AnotoInkTrace.posTag));
                byte[] chunk = new byte[end - (start + AnotoInkTrace.preTag.Length)];
                Array.Copy(tempData, start + AnotoInkTrace.preTag.Length, chunk, 0, chunk.Length);
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
