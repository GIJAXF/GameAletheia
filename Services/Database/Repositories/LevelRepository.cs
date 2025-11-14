using MongoDB.Driver;
using GameAletheiaCross.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System;

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
            Console.WriteLine($" LevelRepository: Buscando nivel {orderNumber}...");
            
            var level = await _levels.Find(l => l.OrderNumber == orderNumber).FirstOrDefaultAsync();
            
            if (level != null)
            {
                Console.WriteLine($" Nivel encontrado: {level.Name}");
                Console.WriteLine($"   NPCIds en BD: {level.NPCIds?.Count ?? 0}");
                
                if (level.NPCIds != null && level.NPCIds.Count > 0)
                {
                    foreach (var npcId in level.NPCIds)
                    {
                        Console.WriteLine($"      - NPC ID: {npcId}");
                    }
                }
                
                await LoadNPCsForLevel(level);
                
                Console.WriteLine($"   NPCs cargados: {level.NPCs?.Count ?? 0}");
                if (level.NPCs != null && level.NPCs.Count > 0)
                {
                    foreach (var npc in level.NPCs)
                    {
                        Console.WriteLine($"        {npc.Name} en ({npc.PositionX}, {npc.PositionY})");
                    }
                }
            }
            else
            {
                Console.WriteLine($" Nivel {orderNumber} no encontrado");
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
            try
            {
                if (level.NPCIds != null && level.NPCIds.Count > 0)
                {
                    Console.WriteLine($"  Cargando NPCs para nivel {level.OrderNumber}...");
                    
                    var filter = Builders<NPC>.Filter.In(npc => npc.Id, level.NPCIds);
                    level.NPCs = await _npcs.Find(filter).ToListAsync();
                    
                    if (level.NPCs == null || level.NPCs.Count == 0)
                    {
                        Console.WriteLine($" ️ No se encontraron NPCs con los IDs proporcionados");
                        level.NPCs = new List<NPC>();
                    }
                    else
                    {
                        Console.WriteLine($" {level.NPCs.Count} NPCs cargados para nivel {level.OrderNumber}");
                    }
                }
                else
                {
                    level.NPCs = new List<NPC>();
                    Console.WriteLine($" Nivel {level.OrderNumber} no tiene NPCs asignados");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error cargando NPCs: {ex.Message}");
                level.NPCs = new List<NPC>();
            }
        }

        public async Task<Level> CreateAsync(Level level)
        {
            await _levels.InsertOneAsync(level);
            Console.WriteLine($" Nivel {level.OrderNumber} creado con ID: {level.Id}");
            return level;
        }

        public async Task<bool> UpdateAsync(string id, Level level)
        {
            Console.WriteLine($"  Actualizando nivel {level.OrderNumber}...");
            Console.WriteLine($"   NPCIds a guardar: {level.NPCIds?.Count ?? 0}");
            
            var result = await _levels.ReplaceOneAsync(l => l.Id == id, level);
            
            Console.WriteLine($"   MatchedCount: {result.MatchedCount}, ModifiedCount: {result.ModifiedCount}");
            
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