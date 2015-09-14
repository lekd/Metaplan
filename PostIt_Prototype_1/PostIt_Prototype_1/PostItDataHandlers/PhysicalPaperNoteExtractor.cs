using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PostIt_Prototype_1.PostItDataHandlers
{
    public class PhysicalPaperNoteExtractor:IPostItDataHandler
    {
        static byte[] standardPrefix = new byte[] { (byte)'<', (byte)'T', (byte)'A', (byte)'B', (byte)'L', (byte)'E', (byte)'-'};
        static byte[] standardPostfix = new byte[] { (byte)'<', (byte)'/', (byte)'T', (byte)'A', (byte)'B', (byte)'L', (byte)'E', (byte)'-' };
    }
}
