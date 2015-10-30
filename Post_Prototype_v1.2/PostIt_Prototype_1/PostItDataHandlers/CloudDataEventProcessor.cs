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
            foreach (int noteID in noteStreams.Keys)
            {
                Stream stream = noteStreams[noteID];
                if (stream is MemoryStream)
                {
                    PostItNote note = new PostItNote();
                    note.Id = noteID;
                    note.CenterX = 0;
                    note.CenterY = 0;
                    note.DataType = PostItContentDataType.WritingImage;
                    note.ParseContentFromBytes(note.DataType, (stream as MemoryStream).ToArray());
                    if (newNoteExtractedEventHandler != null)
                    {
                        newNoteExtractedEventHandler(note);
                    }
                }
            }
        }
    }
}
