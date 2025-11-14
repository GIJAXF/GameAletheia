using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using MongoDB.Driver;

namespace GameAletheiaCross.Services
{
    /// <summary>
    /// Servicio para obtener todos los niveles con sus NPCs asociados
    /// </summary>
    public class LevelsWithNPCsService
    {
        private readonly MongoDbService _dbService;
        private readonly LevelRepository _levelRepo;

        public LevelsWithNPCsService(MongoDbService dbService)
        {
            _dbService = dbService;
            _levelRepo = new LevelRepository(dbService);
        }

        /// <summary>
        /// Obtiene todos los niveles con sus NPCs asociados
        /// </summary>
        public async Task<List<LevelWithNPCsDto>> GetAllLevelsWithNPCsAsync()
        {
            try
            {
                var levels = await _levelRepo.GetAllAsync();
                var result = new List<LevelWithNPCsDto>();

                foreach (var level in levels)
                {
                    result.Add(new LevelWithNPCsDto
                    {
                        LevelId = level.Id,
                        LevelNumber = level.OrderNumber,
                        LevelName = level.Name,
                        Description = level.Description,
                        Difficulty = level.Difficulty,
                        Type = level.Type,
                        NPCs = level.NPCs ?? new List<NPC>()
                    });
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error obteniendo niveles con NPCs: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Obtiene un nivel específico con sus NPCs
        /// </summary>
        public async Task<LevelWithNPCsDto?> GetLevelWithNPCsAsync(int levelNumber)
        {
            try
            {
                var level = await _levelRepo.GetByOrderNumberAsync(levelNumber);
                
                if (level == null)
                    return null;

                return new LevelWithNPCsDto
                {
                    LevelId = level.Id,
                    LevelNumber = level.OrderNumber,
                    LevelName = level.Name,
                    Description = level.Description,
                    Difficulty = level.Difficulty,
                    Type = level.Type,
                    NPCs = level.NPCs ?? new List<NPC>()
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error obteniendo nivel {levelNumber}: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Imprime un reporte formateado de todos los niveles y sus NPCs
        /// </summary>
        public async Task PrintLevelsReportAsync()
        {
            try
            {
                var levels = await GetAllLevelsWithNPCsAsync();

                Console.WriteLine("\n" + new string('=', 80));
                Console.WriteLine("📊 REPORTE DE NIVELES CON NPCs".PadLeft(50));
                Console.WriteLine(new string('=', 80) + "\n");

                foreach (var level in levels)
                {
                    PrintLevelInfo(level);
                }

                Console.WriteLine(new string('=', 80));
                Console.WriteLine($" Total de niveles: {levels.Count}");
                Console.WriteLine(new string('=', 80) + "\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error imprimiendo reporte: {ex.Message}");
            }
        }

        /// <summary>
        /// Imprime la información de un nivel específico
        /// </summary>
        private void PrintLevelInfo(LevelWithNPCsDto level)
        {
            var difficulty = level.Difficulty switch
            {
                1 => "⭐ Fácil",
                2 => "⭐⭐ Normal",
                3 => "⭐⭐⭐ Difícil",
                4 => "⭐⭐⭐⭐ Muy Difícil",
                5 => "⭐⭐⭐⭐⭐ Extremo",
                _ => "Desconocido"
            };

            Console.WriteLine($"┌─────────────────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ Nivel {level.LevelNumber}: {level.LevelName.PadRight(50)} │");
            Console.WriteLine($"├─────────────────────────────────────────────────────────────────────┤");
            Console.WriteLine($"│ Tipo: {level.Type.PadRight(60)} │");
            Console.WriteLine($"│ Dificultad: {difficulty.PadRight(56)} │");
            Console.WriteLine($"│ Descripción: {level.Description.PadRight(52)} │");
            Console.WriteLine($"│ NPCs en este nivel: {level.NPCs?.Count.ToString().PadRight(45)} │");
            Console.WriteLine($"├─────────────────────────────────────────────────────────────────────┤");

            if (level.NPCs != null && level.NPCs.Count > 0)
            {
                Console.WriteLine($"│ PERSONAJES:                                                         │");
                
                foreach (var npc in level.NPCs)
                {
// Dentro del foreach NPC:

Console.WriteLine($"│   👤 {npc.Name.PadRight(62)} │");
Console.WriteLine($"│      Rol: {npc.Role.PadRight(59)} │");
Console.WriteLine($"│      Pos: ({npc.PositionX}, {npc.PositionY})".PadRight(68) + "│");

// CORRECCIÓN: manejo adecuado de DialogueList
string preview;

if (npc.DialogueList != null && npc.DialogueList.Count > 0)
{
    var first = npc.DialogueList[0];
    preview = first.Length > 50 ? first.Substring(0, 50) + "…" : first;
}
else
{
    preview = "…";
}

Console.WriteLine($"│      Diálogo: {preview.PadRight(51)} │");

                }
            }
            else
            {
                Console.WriteLine($"│ ⚠️ No hay NPCs asignados a este nivel                              │");
            }

            Console.WriteLine($"└─────────────────────────────────────────────────────────────────────┘\n");
        }

        /// <summary>
        /// Retorna un JSON serializable con todos los niveles y NPCs
        /// </summary>
        public async Task<string> GetLevelsAsJsonAsync()
        {
            try
            {
                var levels = await GetAllLevelsWithNPCsAsync();
                return System.Text.Json.JsonSerializer.Serialize(levels, new System.Text.Json.JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error serializando a JSON: {ex.Message}");
                throw;
            }
        }
    }

    /// <summary>
    /// DTO para transferir información de niveles con NPCs
    /// </summary>
    public class LevelWithNPCsDto
    {
        public string LevelId { get; set; }
        public int LevelNumber { get; set; }
        public string LevelName { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public string Type { get; set; }
        public List<NPC> NPCs { get; set; } = new();
    }
}