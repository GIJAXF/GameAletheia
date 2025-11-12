using MongoDB.Driver;
using GameAletheiaCross.Models;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace GameAletheiaCross.Services.Database.Repositories
{
    public class PuzzleRepository
    {
        private readonly IMongoCollection<Puzzle> _puzzles;

        public PuzzleRepository(MongoDbService dbService)
        {
            _puzzles = dbService.GetCollection<Puzzle>("puzzles");
        }

        public async Task<Puzzle?> GetByIdAsync(string id)
        {
            return await _puzzles.Find(p => p.Id == id).FirstOrDefaultAsync();
        }

        public async Task<Puzzle?> GetByLevelIdAsync(string levelId)
        {
            return await _puzzles.Find(p => p.LevelId == levelId).FirstOrDefaultAsync();
        }

        public async Task<List<Puzzle>> GetAllByLevelIdAsync(string levelId)
        {
            return await _puzzles.Find(p => p.LevelId == levelId).ToListAsync();
        }

        public async Task<List<Puzzle>> GetAllAsync()
        {
            return await _puzzles.Find(_ => true).ToListAsync();
        }

        public async Task<Puzzle> CreateAsync(Puzzle puzzle)
        {
            await _puzzles.InsertOneAsync(puzzle);
            return puzzle;
        }

        public async Task<bool> UpdateAsync(string id, Puzzle puzzle)
        {
            var result = await _puzzles.ReplaceOneAsync(p => p.Id == id, puzzle);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _puzzles.DeleteOneAsync(p => p.Id == id);
            return result.DeletedCount > 0;
        }
    }
}