using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using ReactiveUI;
using System;

namespace GameAletheiaCross.Models
{
    public class Player : ReactiveObject
    {
        private string _id;
        private string _name;
        private string _gender;
        private string _avatar = "default";
        private int _currentLevel = 1;
        private string _faction;
        private int _totalScore = 0;
        private int _health = 100;
        private Position _position = new Position { X = 100, Y = 400 };
        private Velocity _velocity = new Velocity();
        private bool _isJumping;
        private bool _isFacingRight = true;
        private DateTime _creationDate = DateTime.UtcNow;
        private DateTime _lastPlayed = DateTime.UtcNow;

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id
        {
            get => _id;
            set => this.RaiseAndSetIfChanged(ref _id, value);
        }

        [BsonElement("name")]
        public string Name
        {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }

        [BsonElement("gender")]
        public string Gender
        {
            get => _gender;
            set => this.RaiseAndSetIfChanged(ref _gender, value);
        }

        [BsonElement("avatar")]
        public string Avatar
        {
            get => _avatar;
            set => this.RaiseAndSetIfChanged(ref _avatar, value);
        }

        [BsonElement("currentLevel")]
        public int CurrentLevel
        {
            get => _currentLevel;
            set => this.RaiseAndSetIfChanged(ref _currentLevel, value);
        }

        [BsonElement("faction")]
        public string Faction
        {
            get => _faction;
            set => this.RaiseAndSetIfChanged(ref _faction, value);
        }

        [BsonElement("totalScore")]
        public int TotalScore
        {
            get => _totalScore;
            set => this.RaiseAndSetIfChanged(ref _totalScore, value);
        }

        [BsonElement("health")]
        public int Health
        {
            get => _health;
            set => this.RaiseAndSetIfChanged(ref _health, value);
        }

        [BsonElement("position")]
        public Position Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }

        [BsonElement("velocity")]
        public Velocity Velocity
        {
            get => _velocity;
            set => this.RaiseAndSetIfChanged(ref _velocity, value);
        }

        [BsonElement("isJumping")]
        public bool IsJumping
        {
            get => _isJumping;
            set => this.RaiseAndSetIfChanged(ref _isJumping, value);
        }

        [BsonElement("isFacingRight")]
        public bool IsFacingRight
        {
            get => _isFacingRight;
            set => this.RaiseAndSetIfChanged(ref _isFacingRight, value);
        }

        [BsonElement("creationDate")]
        public DateTime CreationDate
        {
            get => _creationDate;
            set => this.RaiseAndSetIfChanged(ref _creationDate, value);
        }

        [BsonElement("lastPlayed")]
        public DateTime LastPlayed
        {
            get => _lastPlayed;
            set => this.RaiseAndSetIfChanged(ref _lastPlayed, value);
        }
    }

    public class Position : ReactiveObject
    {
        private float _x;
        private float _y;

        [BsonElement("x")]
        public float X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        [BsonElement("y")]
        public float Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }
    }

    public class Velocity : ReactiveObject
    {
        private float _x;
        private float _y;

        [BsonElement("x")]
        public float X
        {
            get => _x;
            set => this.RaiseAndSetIfChanged(ref _x, value);
        }

        [BsonElement("y")]
        public float Y
        {
            get => _y;
            set => this.RaiseAndSetIfChanged(ref _y, value);
        }
    }
}