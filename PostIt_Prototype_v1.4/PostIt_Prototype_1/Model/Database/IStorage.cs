using System.IO;
using AppLimit.CloudComputing.SharpBox;
using MongoDB.Bson;

namespace PostIt_Prototype_1.Model.Database
{
    public interface IStorage<T>
    {
        void Close();
        void CreateSession(string sessionName);
        T GetSession(string sessionName);
        void DownloadFile(string name, T session, MemoryStream memStream);
        BsonDocument UploadFile(MemoryStream screenshotStream, string v, T session);
        BsonDocument UploadFile(string localFilePath, T targetFolder);
    }
}