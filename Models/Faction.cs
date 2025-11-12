using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameAletheiaCross.Models
{
    public class Faction
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; }
        
        [BsonElement("type")]
        public string Type { get; set; }
        
        [BsonElement("leader")]
        public string Leader { get; set; }
        
        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("themeColor")]
        public string ThemeColor { get; set; }
    }
}