using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace GameAletheiaCross.Models
{
    public class Decision
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("playerId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string PlayerId { get; set; }
        
        [BsonElement("levelId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string LevelId { get; set; }
        
        [BsonElement("choice")]
        public string Choice { get; set; }
        
        [BsonElement("factionAffected")]
        public string FactionAffected { get; set; }
        
        [BsonElement("factionPoints")]
        public int FactionPoints { get; set; }
        
        [BsonElement("decisionDate")]
        public DateTime DecisionDate { get; set; } = DateTime.UtcNow;
    }
}