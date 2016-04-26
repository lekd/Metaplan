using System.Collections.Generic;
using System.IO;
using AppLimit.CloudComputing.SharpBox;

namespace PostIt_Prototype_1.NetworkCommunicator
{
    public interface INoteUpNDownloader
    {

        event NewNoteStreamsDownloaded noteStreamsDownloadedHandler;

        void Close();
        List<ICloudFileSystemEntry> getUpdatedNotes(string folderPath);
        void UpdateMetaplanBoardScreen(byte[] screenshotBytes);
        void UpdateMetaplanBoardScreen(MemoryStream screenshotStream, int retry = 3);
        void UpdateNotes();
    }

    public delegate void NewNoteStreamsDownloaded(Dictionary<int, Stream> downloadedNoteStream);

}