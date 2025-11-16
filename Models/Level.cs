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
        
        // ⭐ NUEVO: Piso principal (el suelo base del nivel)
        [BsonElement("floor")]
        public Floor FloorPlatform { get; set; } = new Floor();
        
        [BsonElement("platforms")]
        public List<Platform> Platforms { get; set; } = new List<Platform>();
        
        [BsonElement("enemies")]
        public List<Enemy> Enemies { get; set; } = new List<Enemy>();

        [BsonElement("npcs")]
        public List<string> NPCIds { get; set; } = new List<string>();

        [BsonElement("Background")]
        public string Background { get; set; } = "";

        [BsonIgnore]
        public List<NPC> NPCs { get; set; } = new();

        // ⭐ NUEVA CLASE: Piso principal
        public class Floor
        {
            [BsonElement("x")]
            public float X { get; set; } = 0;
            
            [BsonElement("y")]
            public float Y { get; set; } = 600;
            
            [BsonElement("width")]
            public float Width { get; set; } = 1280;
            
            [BsonElement("height")]
            public float Height { get; set; } = 120;
            
            [BsonElement("isSolid")]
            public bool IsSolid { get; set; } = true;
            
            // Tipo de piso para determinar el sprite
            [BsonElement("floorType")]
            public string FloorType { get; set; } = "Pasto"; // Pasto, Hielo, Cristal, RedLine, PiedraTutorial
        }

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