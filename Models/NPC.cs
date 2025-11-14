using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GameAletheiaCross.Models
{
    public class NPC
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("Name")]
        public string Name { get; set; }

        [BsonElement("Role")]
        public string Role { get; set; }

        [BsonElement("FactionId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FactionId { get; set; }

        [BsonElement("PositionX")]
        public float PositionX { get; set; }

        [BsonElement("PositionY")]
        public float PositionY { get; set; }

        [BsonElement("Dialogue")]
        public List<string> DialogueList { get; set; } = new();

        [BsonElement("IsActive")]
        public bool IsActive { get; set; } = true;

        [BsonElement("LevelId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? LevelId { get; set; }     // <-- ACEPTA NULL

        [BsonElement("Stats")]
        public NPCStats Stats { get; set; } = new NPCStats();

        public class NPCStats
        {
            [BsonElement("Strength")]
            public int Strength { get; set; }

            [BsonElement("Defense")]
            public int Defense { get; set; }

            [BsonElement("Intelligence")]
            public int Intelligence { get; set; }

            [BsonElement("Agility")]
            public int Agility { get; set; }
        }
    }
}
