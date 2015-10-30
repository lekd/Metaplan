using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class PostItNetworkDataManager
    {
        AnotoInkManager _anotoInkManager = null;

        public AnotoInkManager AnotoInkManager
        {
            get { return _anotoInkManager; }
            set { _anotoInkManager = value; }
        }
        GenericPostItCommandDecoder _genericPostItDecoder = null;

        public GenericPostItCommandDecoder GenericPostItDecoder
        {
            get { return _genericPostItDecoder; }
            set { _genericPostItDecoder = value; }
        }
        public PostItNetworkDataManager()
        {
            _anotoInkManager = new AnotoInkManager();
            _genericPostItDecoder = new GenericPostItCommandDecoder();
        }
        public void networkReceivedDataHandler(byte[] data)
        {
            if (_anotoInkManager.IsAnotoMessage(data))
            {
                _anotoInkManager.networkReceivedDataHandler(data);
            }
            else
            {
                _genericPostItDecoder.decodeCommandInByteArray(data);
            }
        }
    }
}
