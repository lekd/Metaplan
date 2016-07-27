using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using AppLimit.CloudComputing.SharpBox;

namespace PostIt_Prototype_1.Model.NetworkCommunicator
{
    class AnotoNoteUpdater: NoteUpdater
    {
        public AnotoNoteUpdater() : base(".txt")
        {

        }
        protected override void ProcessMemStream(Dictionary<int, Stream> noteFiles, MemoryStream memStream, int noteId)
        {
            var text2Str = new Utilities.PointStringToBmp(Properties.Settings.Default.AnotoNoteScale);

            var reader = new StreamReader(memStream);
            var pointsStr = reader.ReadToEnd();
            var bmp = text2Str.FromString(pointsStr,
                Properties.Settings.Default.AnotoNoteInitWidth, Properties.Settings.Default.AnotoNoteInitHeight,
                Properties.Settings.Default.AnotoNoteInitLeft, Properties.Settings.Default.AnotoNoteInitTop);
            var bmpBytes = Utilities.UtilitiesLib.BitmapToBytes(bmp);
            using (var bmpMemStream = new MemoryStream(bmpBytes))
            {
                noteFiles.Add(noteId, bmpMemStream);
            }
        }

        internal void Close()
        {
            throw new NotImplementedException();
        }
    }
}
