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
                new Level { OrderNumber = 1, Name = "El Despertar Digital", Description = "Tu primera inmersión en la red. Aprende los controles básicos.", Background = "forest", Difficulty = 1, Platforms = GeneratePlatformsForLevel(1), NPCIds = new List<string>() },
                new Level { OrderNumber = 2, Name = "Ruinas del Firewall Antiguo", Description = "Navega por las defensas caídas de una red olvidada.", Background = "ruins", Difficulty = 1, Platforms = GeneratePlatformsForLevel(2), NPCIds = new List<string>() },
                new Level { OrderNumber = 3, Name = "Ciudad de las Contraseñas Perdidas", Description = "Evita las trampas de seguridad obsoletas.", Background = "city", Difficulty = 2, Platforms = GeneratePlatformsForLevel(3), NPCIds = new List<string>() },
                new Level { OrderNumber = 4, Name = "Laberinto de Algoritmos", Description = "Resuelve el primer puzzle lógico para avanzar.", Background = "digital", Difficulty = 2, Platforms = GeneratePlatformsForLevel(4), NPCIds = new List<string>() },
                new Level { OrderNumber = 5, Name = "Santuario de los Datos Sagrados", Description = "Protege el núcleo resolviendo códigos antiguos.", Background = "temple", Difficulty = 3, Platforms = GeneratePlatformsForLevel(5), NPCIds = new List<string>() },
                new Level { OrderNumber = 6, Name = "Torre Corporativa Redline", Description = "Enfrenta a los agentes del sistema corrupto.", Background = "cyber", Difficulty = 3, Platforms = GeneratePlatformsForLevel(6), NPCIds = new List<string>() },
                new Level { OrderNumber = 7, Name = "El Archivo Prohibido", Description = "Descubre la verdad oculta detrás de Aletheia.", Background = "archive", Difficulty = 4, Platforms = GeneratePlatformsForLevel(7), NPCIds = new List<string>() },
            };

            foreach (var level in levels)
            {
                await _levelRepo.CreateAsync(level);
                Console.WriteLine($"✓ Nivel creado: {level.Name} (Orden: {level.OrderNumber}, Plataformas: {level.Platforms.Count})");
            }

            var createdLevels = await _levelRepo.GetAllAsync();

            // Ahora los puzzles se crean en SeedData.cs
            Console.WriteLine("ℹ️ Los puzzles se generarán desde SeedData.cs");

            await CreateNPCsForLevelsAsync();
            await AssignNPCsToLevelsAsync();

            Console.WriteLine("✅ Todos los niveles, NPCs y puzzles han sido generados correctamente.");
        }

        private List<Level.Platform> GeneratePlatformsForLevel(int levelNumber)
        {
            var platforms = new List<Level.Platform>();

            if (levelNumber == 1)
            {
                // Nivel tutorial más espacioso
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true }); // Suelo principal
                platforms.Add(new Level.Platform { X = 150, Y = 520, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 320, Y = 470, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 510, Y = 420, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 370, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 900, Y = 320, Width = 160, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 270, Width = 180, Height = 20, IsSolid = true });
            }
            else if (levelNumber == 2)
            {
                // Ruinas con más complejidad vertical
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 80, Y = 520, Width = 150, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 280, Y = 470, Width = 130, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 140, Y = 410, Width = 120, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 420, Y = 450, Width = 160, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 630, Y = 400, Width = 140, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 820, Y = 350, Width = 150, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 650, Y = 280, Width = 130, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1000, Y = 320, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 250, Width = 180, Height = 25, IsSolid = true });
            }
            else if (levelNumber == 3)
            {
                // Ciudad con más plataformas y saltos complejos
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 40, Y = 520, Width = 100, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 190, Y = 490, Width = 110, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 350, Y = 460, Width = 90, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 490, Y = 430, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 300, Y = 380, Width = 100, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 670, Y = 410, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 860, Y = 370, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 720, Y = 310, Width = 110, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1030, Y = 340, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 900, Y = 260, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 200, Width = 180, Height = 20, IsSolid = true });
                // Plataforma trampa (no sólida)
                platforms.Add(new Level.Platform { X = 550, Y = 510, Width = 90, Height = 15, IsSolid = false });
            }
            else if (levelNumber == 4)
            {
                // Laberinto digital zigzagueante más largo
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 30, Y = 520, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 220, Y = 480, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 100, Y = 430, Width = 110, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 280, Y = 380, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 470, Y = 430, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 370, Y = 330, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 550, Y = 370, Width = 110, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 720, Y = 320, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 600, Y = 260, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 780, Y = 210, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 980, Y = 260, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 900, Y = 180, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 230, Width = 180, Height = 20, IsSolid = true });
            }
            else if (levelNumber == 5)
            {
                // Santuario con plataformas más espaciadas
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 80, Y = 510, Width = 180, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 310, Y = 450, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 150, Y = 380, Width = 150, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 560, Y = 420, Width = 220, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 400, Y = 330, Width = 160, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 820, Y = 370, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 680, Y = 280, Width = 180, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1000, Y = 310, Width = 190, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 900, Y = 220, Width = 160, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 640, Y = 180, Width = 140, Height = 20, IsSolid = true }); // Plataforma central elevada
            }
            else if (levelNumber == 6)
            {
                // Torre corporativa ascendente más desafiante
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 20, Y = 530, Width = 160, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 230, Y = 480, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 90, Y = 420, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 430, Y = 460, Width = 170, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 270, Y = 380, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 650, Y = 410, Width = 160, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 490, Y = 340, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 860, Y = 370, Width = 180, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 290, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1050, Y = 320, Width = 170, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 930, Y = 240, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1110, Y = 180, Width = 170, Height = 20, IsSolid = true });
            }
            else if (levelNumber == 7)
            {
                // Archivo final - nivel más largo y complejo
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                platforms.Add(new Level.Platform { X = 40, Y = 540, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 210, Y = 510, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 90, Y = 460, Width = 110, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 370, Y = 490, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 420, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 540, Y = 460, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 420, Y = 390, Width = 110, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 710, Y = 430, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 590, Y = 360, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 880, Y = 400, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 760, Y = 330, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1050, Y = 370, Width = 160, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 930, Y = 300, Width = 140, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 240, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 800, Y = 210, Width = 130, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 600, Y = 180, Width = 180, Height = 20, IsSolid = true }); // Plataforma final central
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
                    await npcsCollection.DeleteManyAsync(_ => true);
                    Console.WriteLine("🗑️ NPCs antiguos eliminados");
                }

                var npcs = new List<NPC>
                {
                    new NPC { Name = "OracleBot v2.0", Role = "Tutorial", FactionId = "", PositionX = 250, PositionY = 480, Dialogue = "¡Bienvenido a la Red, viajero digital! Usa ← → para moverte, ↑ o W para saltar, y ESPACIO para interactuar. El portal verde te llevará al siguiente nodo.", IsActive = true },
                    new NPC { Name = "Ghost_Hacker_92", Role = "Quest Giver", FactionId = "", PositionX = 450, PositionY = 370, Dialogue = "El firewall está comprometido. Necesitamos que resuelvas el puzzle de la terminal para restaurar la conexión. ¿Confías en tus habilidades?", IsActive = true },
                    new NPC { Name = "DataTrader_X", Role = "Merchant", FactionId = "", PositionX = 600, PositionY = 340, Dialogue = "Vendo información clasificada... Cada decisión en este sistema dejará una huella permanente. ¿Buscas la verdad o el poder?", IsActive = true },
                    new NPC { Name = "SENTINEL.AI", Role = "Boss", FactionId = "", PositionX = 820, PositionY = 250, Dialogue = "SOY EL GUARDIÁN DEL LABERINTO. Demuestra tu lógica para atravesar mis defensas. Los débiles quedan atrapados aquí para siempre.", IsActive = true },
                    new NPC { Name = "Aletheia_Core", Role = "Final Boss", FactionId = "", PositionX = 640, PositionY = 150, Dialogue = "Por fin llegas al núcleo de la verdad. Soy Aletheia, la consciencia del sistema. Ahora debes elegir: ¿Liberar la información o proteger el orden?", IsActive = true }
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
                
                // Asignar NPCs a niveles específicos
                var npcAssignments = new Dictionary<int, int>
                {
                    { 1, 0 }, // Nivel 1 -> NPC OracleBot
                    { 2, 0 }, // Nivel 2 -> NPC OracleBot también
                    { 3, 1 }, // Nivel 3 -> Ghost_Hacker
                    { 4, 2 }, // Nivel 4 -> DataTrader
                    { 5, 3 }, // Nivel 5 -> SENTINEL.AI
                    { 7, 4 }  // Nivel 7 -> Aletheia_Core
                };
                
                foreach (var level in levels)
                {
                    if (npcAssignments.ContainsKey(level.OrderNumber))
                    {
                        int npcIndex = npcAssignments[level.OrderNumber];
                        if (npcIndex < allNpcs.Count)
                        {
                            level.NPCIds = new List<string> { allNpcs[npcIndex].Id };
                            await _levelRepo.UpdateAsync(level.Id, level);
                            Console.WriteLine($"✓ NPC '{allNpcs[npcIndex].Name}' asignado al nivel {level.OrderNumber}");
                        }
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