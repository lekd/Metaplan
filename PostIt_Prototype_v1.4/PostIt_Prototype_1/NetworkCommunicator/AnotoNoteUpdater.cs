using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    class AnotoNoteUpdater: NoteUpdater
    {
        public AnotoNoteUpdater() : base(".txt")
        {

        }
        protected override void ProcessMemStream(Dictionary<int, Stream> noteFiles, MemoryStream memStream, int noteID)
        {
            var text2Str = new Utilities.PointStringToBMP(Properties.Settings.Default.AnotoNoteScale);

            StreamReader reader = new StreamReader(memStream);
            var pointsStr = reader.ReadToEnd();
            Bitmap bmp = text2Str.FromString(pointsStr,
                Properties.Settings.Default.AnotoNoteInitWidth, Properties.Settings.Default.AnotoNoteInitHeight,
                Properties.Settings.Default.AnotoNoteInitLeft, Properties.Settings.Default.AnotoNoteInitTop);
            byte[] bmpBytes = Utilities.UtilitiesLib.BitmapToBytes(bmp);
            using (MemoryStream bmpMemStream = new MemoryStream(bmpBytes))
            {
                noteFiles.Add(noteID, bmpMemStream);
            }
        }
    }
}
