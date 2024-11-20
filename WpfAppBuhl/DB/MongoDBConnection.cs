using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfAppBuhl.DB
{
    public class MongoDBConnection : IMongoDBConnection
    {
        IMongoClient client;
        private const string ConnectionUri = ""; //Hier MongoDB Uri eintragen!
        const string DatenbankName = "buhl";

        public MongoDBConnection()
        {
            //offizieller code von mongoDB
            var einstellungen = MongoClientSettings.FromConnectionString(ConnectionUri);
            // Set the ServerApi field of the settings object to set the version of the Stable API on the client
            einstellungen.ServerApi = new ServerApi(ServerApiVersion.V1);
            // Create a new client and connect to the server
            client = new MongoClient(einstellungen);
        }
        
        //Bestimmen, welche DB genutzt wird
        public IMongoDatabase GetDatabase()
        {
            return client.GetDatabase(DatenbankName);          
        }

        //Bestimmt die Collection von MongoDB
        public IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return GetDatabase().GetCollection<T>(collectionName);
        }

        
    }
}
