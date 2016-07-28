using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class CloudDataEventProcessor
    {
        public delegate void NewNoteExtractedFromStreamEvent(PostItNote note);

        public event NewNoteExtractedFromStreamEvent newNoteExtractedEventHandler = null;

        public void handleDownloadedStreamsFromCloud(Dictionary<int, Stream> noteStreams)
        {
            foreach (var noteID in noteStreams.Keys)
            {
                var stream = noteStreams[noteID];
                if (stream is MemoryStream)
                {
                    var note = new PostItNote
                    {
                        Id = noteID,
                        CenterX = 0,
                        CenterY = 0,
                        DataType = PostItContentDataType.WritingImage
                    };
                    note.ParseContentFromBytes(note.DataType, (stream as MemoryStream).ToArray());
                    newNoteExtractedEventHandler?.Invoke(note);
                }
            }
        }
    }
}
