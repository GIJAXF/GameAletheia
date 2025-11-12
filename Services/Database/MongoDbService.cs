using MongoDB.Driver;
using System;

namespace GameAletheiaCross.Services.Database
{
    public class MongoDbService
    {
        private readonly IMongoDatabase _database;

        public MongoDbService(string connectionString = "mongodb://localhost:27017", string dbName = "HackerFantasmaDB")
        {
            var client = new MongoClient(connectionString);
            _database = client.GetDatabase(dbName);
        }

        // Devuelve colecciones fuertemente tipadas
        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }

        // Ping sencillo
        public bool Ping()
        {
            try
            {
                _database.RunCommandAsync((Command<MongoDB.Bson.BsonDocument>)"{ping:1}").Wait();
                Console.WriteLine("✓ Conexión a MongoDB exitosa");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error de conexión: {ex.Message}");
                return false;
            }
        }
    }
}
