using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class NetworkDataProcessor
    {
        AnotoInkManager anotoInkMng = null;
        IPostItNetworkDataHandler ClassifyIncomingData(byte[] incomingData)
        {
            string dataStr = Encoding.UTF8.GetString(incomingData);
            if(AnotoInkTrace.CheckIsAnotoTraceMessage(dataStr))
            {
                return anotoInkMng;
            }
            
            return null;
        }
    }
}
