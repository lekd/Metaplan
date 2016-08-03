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

        public event NewNoteExtractedFromStreamEvent NewNoteExtractedEventHandler = null;

        public void HandleDownloadedStreamsFromCloud(int noteId, Stream downloadedNoteStream)
        {
            var stream = downloadedNoteStream as MemoryStream;
            if (stream == null)
                return;

            var note = new PostItNote
            {
                Id = noteId,
                CenterX = 0,
                CenterY = 0,
                DataType = PostItContentDataType.WritingImage
            };
            note.ParseContentFromBytes(note.DataType, stream.ToArray());
            NewNoteExtractedEventHandler?.Invoke(note);
        }
    }
}
