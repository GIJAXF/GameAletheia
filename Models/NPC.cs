using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameAletheiaCross.Models
{
    public class NPC
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; }
        
        [BsonElement("role")]
        public string Role { get; set; }
        
        [BsonElement("factionId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FactionId { get; set; }
        
        [BsonElement("positionX")]
        public float PositionX { get; set; }
        
        [BsonElement("positionY")]
        public float PositionY { get; set; }
        
        [BsonElement("dialogue")]
        public string Dialogue { get; set; }
        
        [BsonElement("isActive")]
        public bool IsActive { get; set; } = true;
    }
}