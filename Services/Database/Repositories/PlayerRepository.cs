using MongoDB.Driver;
using GameAletheiaCross.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

namespace GameAletheiaCross.Services.Database.Repositories
{
    public class PlayerRepository
    {
        private readonly IMongoCollection<Player> _players;

        public PlayerRepository(MongoDbService dbService)
        {
            _players = dbService.GetCollection<Player>("players");
        }

        public async Task<Player?> GetByIdAsync(string id)
        {
            var player = await _players.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (player != null)
            {
                Console.WriteLine($"🔍 GetByIdAsync: Jugador '{player.Name}' encontrado (Nivel {player.CurrentLevel})");
            }
            else
            {
                Console.WriteLine($"❌ GetByIdAsync: Jugador con ID {id} no encontrado");
            }
            return player;
        }

        public async Task<Player?> GetByNameAsync(string name)
        {
            return await _players.Find(p => p.Name == name).FirstOrDefaultAsync();
        }

        public async Task<List<Player>> GetAllAsync()
        {
            return await _players.Find(_ => true).ToListAsync();
        }

        public async Task<List<Player>> GetTopPlayersByScoreAsync(int limit = 10)
        {
            return await _players.Find(_ => true)
                .SortByDescending(p => p.TotalScore)
                .Limit(limit)
                .ToListAsync();
        }

        public async Task<Player> CreateAsync(Player player)
        {
            await _players.InsertOneAsync(player);
            Console.WriteLine($"✅ CreateAsync: Jugador '{player.Name}' creado con ID {player.Id}");
            return player;
        }

        public async Task<bool> UpdateAsync(string id, Player player)
        {
            player.LastPlayed = DateTime.UtcNow;
            
            Console.WriteLine($"🔄 UpdateAsync ANTES: Jugador '{player.Name}' en nivel {player.CurrentLevel}");
            
            var result = await _players.ReplaceOneAsync(p => p.Id == id, player);
            
            Console.WriteLine($"🔄 UpdateAsync DESPUÉS: MatchedCount={result.MatchedCount}, ModifiedCount={result.ModifiedCount}");
            
            // Verificar que se actualizó correctamente
            if (result.ModifiedCount > 0)
            {
                var verificacion = await GetByIdAsync(id);
                if (verificacion != null)
                {
                    Console.WriteLine($"✅ Verificación: Jugador ahora está en nivel {verificacion.CurrentLevel}");
                }
            }
            
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateFactionAsync(string id, string factionName)
        {
            var update = Builders<Player>.Update.Set(p => p.Faction, factionName);
            var result = await _players.UpdateOneAsync(p => p.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdateScoreAsync(string id, int scoreToAdd)
        {
            var player = await GetByIdAsync(id);
            if (player == null) return false;

            player.TotalScore += scoreToAdd;
            return await UpdateAsync(id, player);
        }

        public async Task<bool> UpdateHealthAsync(string id, int newHealth)
        {
            var update = Builders<Player>.Update.Set(p => p.Health, newHealth);
            var result = await _players.UpdateOneAsync(p => p.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> UpdatePositionAsync(string id, float x, float y)
        {
            var update = Builders<Player>.Update
                .Set("position.x", x)
                .Set("position.y", y);
            var result = await _players.UpdateOneAsync(p => p.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _players.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }
    }
}