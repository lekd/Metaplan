namespace PostIt_Prototype_1.NetworkCommunicator
{
    public interface IAnotoNotesDownloader
    {
        event NewNoteStreamsDownloaded noteStreamsDownloadedHandler;

        void Close();
        void UpdateNotes();
    }
}