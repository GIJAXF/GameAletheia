using System;
using MongoDB.Bson;
using MongoDB.Driver;
using GameAletheiaCross.Services.Database;


public class MongoTest
{
    public static void Run()
    {
        var db = new MongoDbService();
        var test = db.GetCollection<BsonDocument>("test");
        
        test.InsertOne(new BsonDocument { { "msg", "hola desde Avalonia" } });
        
        Console.WriteLine("Mongo conectado y funcionando.");
    }
}
