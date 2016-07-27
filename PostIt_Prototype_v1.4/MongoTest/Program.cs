using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;

namespace MongoTest
{
    class Program
    {
        static void Main(string[] args)
        {
            
        }

        private static void TestMongo()
        {
            const string dbuser = "ali";
            const string dbpassword = "ali123";
            var client = new MongoClient($"mongodb://{dbuser}:{dbpassword}@ds021895.mlab.com:21895/metaplan");
            var database = client.GetDatabase("metaplan");
            var notes = database.GetCollection<BsonDocument>("Notes");
            var newNote = new BsonDocument
            {
                {"user", "Ali" },
                {"content", "Some text" },
                {"background", "Yellow" },
                {"position", new BsonDocument
                    {
                        { "x", 10 },
                        {"y", 20}
                }},
                {"rotation", 0 }
            };
            notes.InsertOne(newNote);
        }
    }
}
