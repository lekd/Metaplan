using System;
using System.IO;
using System.Linq;
using AppLimit.CloudComputing.SharpBox;
using MongoDB.Bson;
using MongoDB.Driver;

namespace PostIt_Prototype_1.Model.Database
{
    public class MongoStorage : IStorage<MongoDB.Bson.BsonDocument>
    {
        private readonly IMongoDatabase _database;

        private IMongoCollection<BsonDocument> Sessions => _database.GetCollection<MongoDB.Bson.BsonDocument>("Sessions");
        private IMongoCollection<BsonDocument> Notes => _database.GetCollection<MongoDB.Bson.BsonDocument>("Notes");
        private IMongoCollection<BsonDocument> Files => _database.GetCollection<MongoDB.Bson.BsonDocument>("Files");

        public MongoStorage()
        {
            const string dbuser = "ali";
            const string dbpassword = "ali123";
            var client = new MongoClient($"mongodb://{dbuser}:{dbpassword}@ds021895.mlab.com:21895/metaplan");
            _database = client.GetDatabase("metaplan");

        }

        public void Close()
        {
            throw new NotImplementedException();
        }

        public void CreateSession(string sessionName)
        {
            if (GetSession(sessionName) != null)
                Sessions.InsertOne(new BsonDocument
                {
                    {"name", sessionName },
                    {"owner", SessionOwner },
                    {"notes", null }
                });
        }

        public const String SessionOwner = "TEST_OWNER";

        public BsonDocument GetSession(string sessionName)
        {
            return Sessions.Find(d => d["name"] == sessionName).First();
        }

        public void DownloadFile(string name, BsonDocument session, MemoryStream memStream)
        {
            var bytes = Notes.Find(d => d["name"] == name && d["sessionId"] == session["Id"]).
                First()["content"].
                AsBsonBinaryData.Bytes;

            memStream.Write(bytes, 0, bytes.Length);
        }

        public BsonDocument UploadFile(MemoryStream screenshotStream, string name, BsonDocument session)
        {
            var d = new BsonDocument
            {
                {"name", name},
                {"type", "File"},
                {"content", screenshotStream.ToArray() }
            };
            Files.InsertOne(d);
            return d;
        }

        public void UploadFile(string localFilePath, BsonDocument targetFolder)
        {
            var d = new BsonDocument
            {
                {"name", localFilePath.Split('/').Last()},
                {"type", "File"},
                {"content", new StreamReader(localFilePath).ReadToEndAsync().Result}
            };
            Files.InsertOne(d);
        }

        public void CreateLog(string userstudyLog)
        {
            throw new NotImplementedException();
        }

        public BsonDocument GetLog(string logName)
        {
            throw new NotImplementedException();
        }

        BsonDocument IStorage<BsonDocument>.UploadFile(string localFilePath, BsonDocument targetFolder)
        {
            throw new NotImplementedException();
        }
    }
}