using MongoDB.Driver;
using GameAletheiaCross.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace GameAletheiaCross.Services.Database.Repositories
{
    public class LevelRepository
    {
        private readonly IMongoCollection<Level> _levels;
        private readonly IMongoCollection<NPC> _npcs;

        public LevelRepository(MongoDbService dbService)
        {
            _levels = dbService.GetCollection<Level>("levels");
            _npcs = dbService.GetCollection<NPC>("npcs");
        }

        public async Task<Level?> GetByIdAsync(string id)
        {
            var level = await _levels.Find(l => l.Id == id).FirstOrDefaultAsync();
            if (level != null)
            {
                await LoadNPCsForLevel(level);
            }
            return level;
        }

        public async Task<Level?> GetByOrderNumberAsync(int orderNumber)
        {
            var level = await _levels.Find(l => l.OrderNumber == orderNumber).FirstOrDefaultAsync();
            if (level != null)
            {
                await LoadNPCsForLevel(level);
            }
            return level;
        }

        public async Task<List<Level>> GetAllAsync()
        {
            var levels = await _levels.Find(_ => true).ToListAsync();
            foreach (var level in levels)
            {
                await LoadNPCsForLevel(level);
            }
            return levels;
        }

        private async Task LoadNPCsForLevel(Level level)
        {
            if (level.NPCIds != null && level.NPCIds.Count > 0)
            {
                var filter = Builders<NPC>.Filter.In(npc => npc.Id, level.NPCIds);
                level.NPCs = await _npcs.Find(filter).ToListAsync();
            }
        }

        public async Task<Level> CreateAsync(Level level)
        {
            await _levels.InsertOneAsync(level);
            return level;
        }

        public async Task<bool> UpdateAsync(string id, Level level)
        {
            var result = await _levels.ReplaceOneAsync(l => l.Id == id, level);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _levels.DeleteOneAsync(l => l.Id == id);
            return result.DeletedCount > 0;
        }

        public async Task<int> GetTotalLevelsAsync()
        {
            return (int)await _levels.CountDocumentsAsync(_ => true);
        }
    }
}