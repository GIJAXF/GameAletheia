using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GameAletheiaCross.Models
{
    public class Level
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = "";
        
        [BsonElement("name")]
        public string Name { get; set; } = "";
        
        [BsonElement("type")]
        public string Type { get; set; } = "";
        
        [BsonElement("difficulty")]
        public int Difficulty { get; set; }
        
        [BsonElement("orderNumber")]
        public int OrderNumber { get; set; }
        
        [BsonElement("description")]
        public string Description { get; set; } = "";
        
        [BsonElement("timeLimit")]
        public int TimeLimit { get; set; }
        
        [BsonElement("platforms")]
        public List<Platform> Platforms { get; set; } = new List<Platform>();
        
        [BsonElement("floor")]
        public Platform? Floor { get; set; }
        
        [BsonElement("enemies")]
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();

        [BsonElement("npcs")]
        public List<string> NPCIds { get; set; } = new List<string>();

        [BsonElement("Background")]
        public string Background { get; set; } = "";


        [BsonIgnore]
        public List<NPC> NPCs { get; set; } = new();

        //   ESTAS CLASES ANIDADAS SON IMPORTANTES
        public class Platform
        {
            [BsonElement("x")]
            public float X { get; set; }
            
            [BsonElement("y")]
            public float Y { get; set; }
            
            [BsonElement("width")]
            public float Width { get; set; }
            
            [BsonElement("height")]
            public float Height { get; set; }
            
            [BsonElement("isSolid")]
            public bool IsSolid { get; set; } = true;
        }
        
        public class Enemy
        {
            [BsonElement("x")]
            public float X { get; set; }
            
            [BsonElement("y")]
            public float Y { get; set; }
            
            [BsonElement("type")]
            public string Type { get; set; } = "";

            [BsonElement("patrolRoute")]
            public List<string> PatrolRoute { get; set; } = new List<string>();
        }
    }
}