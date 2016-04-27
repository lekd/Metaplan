using System;
using System.Collections.Generic;
using System.IO;

namespace PostIt_Prototype_1.Presentation
{
    internal class NewNoteStreamsDownloaded
    {
        private Action<Dictionary<int, Stream>> handleDownloadedStreamsFromCloud;

        public NewNoteStreamsDownloaded(Action<Dictionary<int, Stream>> handleDownloadedStreamsFromCloud)
        {
            this.handleDownloadedStreamsFromCloud = handleDownloadedStreamsFromCloud;
        }
    }
}