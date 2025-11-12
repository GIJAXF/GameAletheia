using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace GameAletheiaCross.Models
{
    public class Puzzle
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        
        [BsonElement("name")]
        public string Name { get; set; }
        
        [BsonElement("type")]
        public string Type { get; set; }
        
        [BsonElement("description")]
        public string Description { get; set; }
        
        [BsonElement("expectedOutput")]
        public string ExpectedOutput { get; set; }
        
        [BsonElement("starterCode")]
        public string StarterCode { get; set; }
        
        [BsonElement("levelId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string LevelId { get; set; }
        
        [BsonElement("difficulty")]
        public int Difficulty { get; set; }
        
        [BsonElement("hints")]
        public List<string> Hints { get; set; } = new List<string>();

        [BsonElement("points")]
        public int Points { get; set; } = 100;
        
        [BsonElement("isCompleted")]
        public bool IsCompleted { get; set; } = false;
    }
}