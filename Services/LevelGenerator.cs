using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using MongoDB.Driver;

namespace GameAletheiaCross.Services
{
    public class LevelGenerator
    {
        private readonly LevelRepository _levelRepo;
        private readonly PuzzleRepository _puzzleRepo;

        public LevelGenerator(LevelRepository levelRepo, PuzzleRepository puzzleRepo)
        {
            _levelRepo = levelRepo;
            _puzzleRepo = puzzleRepo;
        }

        public async Task GenerateDefaultLevelsAsync()
        {
            var existingCount = await _levelRepo.GetTotalLevelsAsync();
            if (existingCount > 0)
            {
                Console.WriteLine($"⚠️ Ya existen {existingCount} niveles. Eliminando para regenerar...");
                await DeleteAllLevelsAsync();
            }

            var levels = new List<Level>
            {
                new Level { OrderNumber = 1, Name = "El Inicio", Description = "Aprende los controles básicos.", Background = "forest", Difficulty = 1, Platforms = GeneratePlatformsForLevel(1), NPCIds = new List<string>() },
                new Level { OrderNumber = 2, Name = "Ruinas Perdidas", Description = "Encuentra el primer artefacto.", Background = "ruins", Difficulty = 1, Platforms = GeneratePlatformsForLevel(2), NPCIds = new List<string>() },
                new Level { OrderNumber = 3, Name = "Ciudad del Silencio", Description = "Evita las trampas de la ciudad.", Background = "city", Difficulty = 2, Platforms = GeneratePlatformsForLevel(3), NPCIds = new List<string>() },
                new Level { OrderNumber = 4, Name = "Laberinto Digital", Description = "Resuelve el primer puzzle lógico.", Background = "digital", Difficulty = 2, Platforms = GeneratePlatformsForLevel(4), NPCIds = new List<string>() },
                new Level { OrderNumber = 5, Name = "Santuario de Datos", Description = "Defiende el núcleo resolviendo un código.", Background = "temple", Difficulty = 3, Platforms = GeneratePlatformsForLevel(5), NPCIds = new List<string>() },
                new Level { OrderNumber = 6, Name = "Redline", Description = "Enfrenta a los agentes de la corporación.", Background = "cyber", Difficulty = 3, Platforms = GeneratePlatformsForLevel(6), NPCIds = new List<string>() },
                new Level { OrderNumber = 7, Name = "El Archivo", Description = "Descubre la verdad detrás de Aletheia.", Background = "archive", Difficulty = 3, Platforms = GeneratePlatformsForLevel(7), NPCIds = new List<string>() },
            };

            foreach (var level in levels)
            {
                await _levelRepo.CreateAsync(level);
                Console.WriteLine($"✓ Nivel creado: {level.Name} (Orden: {level.OrderNumber}, Plataformas: {level.Platforms.Count})");
            }

            var createdLevels = await _levelRepo.GetAllAsync();

            foreach (var level in createdLevels)
            {
                if (level.OrderNumber == 4)
                {
                    var puzzle = new Puzzle
                    {
                        LevelId = level.Id,
                        Name = "Puzzle de Conexiones",
                        Description = "Reconecta los nodos digitales en el orden correcto.",
                        Points = 150,
                        Type = "Lógico",
                        IsCompleted = false
                    };
                    await _puzzleRepo.CreateAsync(puzzle);
                    Console.WriteLine($"🧩 Puzzle agregado al nivel {level.OrderNumber}: {puzzle.Name}");
                }
                else if (level.OrderNumber == 5)
                {
                    var puzzle = new Puzzle
                    {
                        LevelId = level.Id,
                        Name = "Código del Santuario",
                        Description = "Descifra el patrón de datos oculto.",
                        Points = 200,
                        Type = "Criptografía",
                        IsCompleted = false
                    };
                    await _puzzleRepo.CreateAsync(puzzle);
                    Console.WriteLine($"🧩 Puzzle agregado al nivel {level.OrderNumber}: {puzzle.Name}");
                }
            }

            await CreateNPCsForLevelsAsync();
            await AssignNPCsToLevelsAsync();

            Console.WriteLine("✅ Todos los niveles, NPCs y puzzles han sido generados correctamente.");
        }

        private List<Level.Platform> GeneratePlatformsForLevel(int levelNumber)
        {
            var platforms = new List<Level.Platform>();

            if (levelNumber == 1)
            {
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 200, Y = 520, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 450, Y = 450, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 380, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1000, Y = 300, Width = 200, Height = 20, IsSolid = true });
            }
            else if (levelNumber == 2)
            {
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 100, Y = 500, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 350, Y = 450, Width = 180, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 580, Y = 400, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 850, Y = 350, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1050, Y = 300, Width = 200, Height = 25, IsSolid = true });
            }
            else if (levelNumber == 3)
            {
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 50, Y = 500, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 480, Width = 100, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 450, Y = 420, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 380, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 900, Y = 350, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 300, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 600, Y = 500, Width = 100, Height = 15, IsSolid = false });
            }
            else if (levelNumber == 4)
            {
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 50, Y = 500, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 300, Y = 450, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 150, Y = 400, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 400, Y = 350, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 300, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 500, Y = 250, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 800, Y = 200, Width = 200, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1000, Y = 300, Width = 200, Height = 20, IsSolid = true });
            }
            else if (levelNumber == 5)
            {
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 100, Y = 500, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 350, Y = 430, Width = 250, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 380, Width = 250, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1000, Y = 300, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 640, Y = 200, Width = 100, Height = 20, IsSolid = true });
            }
            else if (levelNumber == 6)
            {
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 0, Y = 500, Width = 180, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 450, Width = 200, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 550, Y = 400, Width = 180, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 850, Y = 350, Width = 200, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 280, Width = 180, Height = 20, IsSolid = true });
            }
            else if (levelNumber == 7)
            {
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 50, Y = 520, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 480, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 450, Y = 440, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 650, Y = 400, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 850, Y = 360, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1050, Y = 320, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 600, Y = 200, Width = 200, Height = 20, IsSolid = true });
            }

            return platforms;
        }

        private async Task DeleteAllLevelsAsync()
        {
            try
            {
                var allLevels = await _levelRepo.GetAllAsync();
                foreach (var level in allLevels)
                {
                    await _levelRepo.DeleteAsync(level.Id);
                }
                Console.WriteLine($"🗑️ {allLevels.Count} niveles antiguos eliminados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error al eliminar niveles: {ex.Message}");
            }
        }

        private async Task CreateNPCsForLevelsAsync()
        {
            try
            {
                var dbService = new MongoDbService();
                var npcsCollection = dbService.GetCollection<NPC>("npcs");
                
                var existingNpcs = await npcsCollection.CountDocumentsAsync(_ => true);
                if (existingNpcs > 0)
                {
                    Console.WriteLine("⚠️ Los NPCs ya existen, saltando...");
                    return;
                }

                var npcs = new List<NPC>
                {
                    new NPC { Name = "Guía Digital", Role = "Tutorial", FactionId = "", PositionX = 300, PositionY = 400, Dialogue = "¡Bienvenido a Aletheia! Usa las flechas para moverte, ARRIBA o W para saltar, y ESPACIO para interactuar. ¡Buena suerte!", IsActive = true },
                    new NPC { Name = "Hacker Misterioso", Role = "Quest Giver", FactionId = "", PositionX = 600, PositionY = 350, Dialogue = "La terminal está bloqueada. Debes resolver el puzzle de programación para continuar. ¿Te atreves?", IsActive = true },
                    new NPC { Name = "Comerciante de Datos", Role = "Merchant", FactionId = "", PositionX = 400, PositionY = 420, Dialogue = "Vendo información valiosa. Cada decisión que tomes en este mundo afectará tu destino.", IsActive = true },
                    new NPC { Name = "Inteligencia Artificial", Role = "Boss", FactionId = "", PositionX = 800, PositionY = 300, Dialogue = "Soy la IA guardiana del laberinto. Debes demostrar tu ingenio para pasar.", IsActive = true },
                    new NPC { Name = "Guardián del Núcleo", Role = "Final Boss", FactionId = "", PositionX = 640, PositionY = 250, Dialogue = "Has llegado al corazón de Aletheia. Ahora debes elegir tu destino...", IsActive = true }
                };

                await npcsCollection.InsertManyAsync(npcs);
                Console.WriteLine($"✓ {npcs.Count} NPCs creados");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error creando NPCs: {ex.Message}");
            }
        }

        private async Task AssignNPCsToLevelsAsync()
        {
            try
            {
                var dbService = new MongoDbService();
                var npcsCollection = dbService.GetCollection<NPC>("npcs");
                
                var allNpcs = await npcsCollection.Find(_ => true).ToListAsync();
                
                if (allNpcs.Count < 5)
                {
                    Console.WriteLine("⚠️ No hay suficientes NPCs para asignar");
                    return;
                }

                var levels = await _levelRepo.GetAllAsync();
                
                for (int i = 0; i < Math.Min(levels.Count, 5); i++)
                {
                    if (i < allNpcs.Count)
                    {
                        levels[i].NPCIds = new List<string> { allNpcs[i].Id };
                        await _levelRepo.UpdateAsync(levels[i].Id, levels[i]);
                        Console.WriteLine($"✓ NPC '{allNpcs[i].Name}' asignado al nivel {levels[i].OrderNumber}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error asignando NPCs: {ex.Message}");
            }
        }
    }
}