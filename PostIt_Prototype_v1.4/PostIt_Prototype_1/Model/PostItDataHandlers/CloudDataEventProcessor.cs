using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using PostIt_Prototype_1.ModelView.PostItObjects;
using PostIt_Prototype_1.PostItObjects;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class CloudDataEventProcessor
    {
        public delegate void NewNoteExtractedFromStreamEvent(PostItNote note);

        public event NewNoteExtractedFromStreamEvent NewNoteExtractedEventHandler = null;

        public void HandleDownloadedStreamsFromCloud(Dictionary<int, Stream> noteStreams)
        {
            foreach (var noteId in noteStreams.Keys)
            {
                var stream = noteStreams[noteId];
                if (stream is MemoryStream)
                {
                    var note = new PostItNote();
                    note.Id = noteId;
                    note.CenterX = 0;
                    note.CenterY = 0;
                    note.DataType = PostItContentDataType.WritingImage;
                    note.ParseContentFromBytes(note.DataType, (stream as MemoryStream).ToArray());
                    if (NewNoteExtractedEventHandler != null)
                    {
                        NewNoteExtractedEventHandler(note);
                    }
                }
            }
        }
    }
}
