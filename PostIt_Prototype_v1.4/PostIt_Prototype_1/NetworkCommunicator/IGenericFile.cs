using System;
using System.Collections.Generic;
using Dropbox.Api.Sharing;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public interface IGenericFile
    {
        DateTime Modified { get; set; }
        string Name { get; set; }
        IGenericFolder Parent { get; set; }
    }
    public interface IGenericFolder
    {
        DateTime Modified { get; set; }
        string Name { get; set; }
        IGenericFolder Parent { get; set; }
        IEnumerable<IGenericFile> GetFilesInFolder();
    }
}