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

            // PRIMERO: Crear y guardar NPCs
            Console.WriteLine("👾 Creando NPCs...");
            await CreateNPCsAsync();

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

            // SEGUNDO: Crear niveles
            Console.WriteLine("🗺️ Creando niveles...");
            foreach (var level in levels)
            {
                await _levelRepo.CreateAsync(level);
                Console.WriteLine($"  ✓ Nivel creado: {level.Name} (Orden: {level.OrderNumber}, Plataformas: {level.Platforms.Count})");
            }

            // TERCERO: Asignar NPCs a niveles
            Console.WriteLine("🔗 Asignando NPCs a niveles...");
            await AssignNPCsToLevelsAsync();

            Console.WriteLine(" Los puzzles se generarán desde SeedData.cs");
            Console.WriteLine(" Todos los niveles y NPCs han sido generados correctamente.");
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

private async Task CreateNPCsAsync()
{
    try
    {
        var dbService = new MongoDbService();
        var npcsCollection = dbService.GetCollection<NPC>("npcs");
        
        // Eliminar NPCs existentes
        var existingCount = await npcsCollection.CountDocumentsAsync(_ => true);
        if (existingCount > 0)
        {
            await npcsCollection.DeleteManyAsync(_ => true);
            Console.WriteLine($"🗑️ {existingCount} NPCs antiguos eliminados");
        }

        // ============================
        // NPCs DEFINITIVOS
        // CON POSICIÓN ASIGNADA
        // ============================
        var npcs = new List<NPC>
        {
            // ======================================
            // FACCIÓN GOBIERNO (001)
            // ======================================

            new NPC
            {
                Id = "671000000000000000000101",
                Name = "El Archivero — Julián Casablancas",
                Role = "Líder del Gobierno",
                FactionId = "671000000000000000000001",
                LevelId = null, // Se asigna después
                PositionX = 620,
                PositionY = 260,    // Punto elevado: observador, vigilante
                DialogueList = new List<string>
                {
                    "El caos no es libertad; es olvido.",
                    "La información debe ser preservada, incluso de ti mismo."
                },
                Stats = new NPC.NPCStats { Strength = 6, Defense = 12, Intelligence = 18, Agility = 5 },
                IsActive = true
            },

            new NPC
            {
                Id = "671000000000000000000102",
                Name = "Custodio Alfa",
                Role = "Agente del Archivo",
                FactionId = "671000000000000000000001",
                LevelId = null,
                PositionX = 540,
                PositionY = 410,   // Vigilante, intermedio
                DialogueList = new List<string>
                {
                    "Acceso restringido. Tu presencia es una anomalía.",
                    "Archivar es purificar."
                },
                Stats = new NPC.NPCStats { Strength = 10, Defense = 9, Intelligence = 12, Agility = 8 },
                IsActive = true
            },

            new NPC
            {
                Id = "671000000000000000000103",
                Name = "Reportero Fantasma",
                Role = "Analista del Gobierno",
                FactionId = "671000000000000000000001",
                LevelId = null,
                PositionX = 880,
                PositionY = 300,
                DialogueList = new List<string>
                {
                    "Todas tus decisiones serán registradas.",
                    "El sistema observa incluso sus propios errores."
                },
                Stats = new NPC.NPCStats { Strength = 4, Defense = 6, Intelligence = 16, Agility = 7 },
                IsActive = true
            },

            // ======================================
            // FACCIÓN REDLINE (002)
            // ======================================

            new NPC
            {
                Id = "671000000000000000000201",
                Name = "Decano Villanueva",
                Role = "Líder de Redline",
                FactionId = "671000000000000000000002",
                LevelId = null,
                PositionX = 400,
                PositionY = 310,
                DialogueList = new List<string>
                {
                    "Todo tiene un precio, incluso tú.",
                    "La red es un negocio… y tú eres inversión."
                },
                Stats = new NPC.NPCStats { Strength = 8, Defense = 14, Intelligence = 13, Agility = 6 },
                IsActive = true
            },

            new NPC
            {
                Id = "671000000000000000000202",
                Name = "IA Centinela R-07",
                Role = "Defensa Automatizada",
                FactionId = "671000000000000000000002",
                LevelId = null,
                PositionX = 700,
                PositionY = 350,
                DialogueList = new List<string>
                {
                    "Directiva: Eliminación de intrusos.",
                    "El beneficio supera al riesgo."
                },
                Stats = new NPC.NPCStats { Strength = 14, Defense = 10, Intelligence = 8, Agility = 4 },
                IsActive = true
            },

            new NPC
            {
                Id = "671000000000000000000203",
                Name = "Analista Fractal",
                Role = "Ingeniero de Redline",
                FactionId = "671000000000000000000002",
                LevelId = null,
                PositionX = 520,
                PositionY = 280,
                DialogueList = new List<string>
                {
                    "Los números no mienten, pero tú sí.",
                    "Redline optimiza… incluso tus fallos."
                },
                Stats = new NPC.NPCStats { Strength = 5, Defense = 7, Intelligence = 15, Agility = 7 },
                IsActive = true
            },

            // ======================================
            // FACCIÓN RESISTENCIA (003)
            // ======================================

            new NPC
            {
                Id = "671000000000000000000301",
                Name = "Noa Espectra",
                Role = "Líder de la Resistencia",
                FactionId = "671000000000000000000003",
                LevelId = null,
                PositionX = 150,
                PositionY = 380,
                DialogueList = new List<string>
                {
                    "La red no pertenece a nadie.",
                    "No temas ensuciarte las manos por la libertad."
                },
                Stats = new NPC.NPCStats { Strength = 7, Defense = 9, Intelligence = 17, Agility = 11 },
                IsActive = true
            },

            new NPC
            {
                Id = "671000000000000000000302",
                Name = "Bibliotecario Errante",
                Role = "Recolector de Datos Libres",
                FactionId = "671000000000000000000003",
                LevelId = null,
                PositionX = 300,
                PositionY = 330,
                DialogueList = new List<string>
                {
                    "La información nace para circular.",
                    "Romper cadenas también rompe certezas."
                },
                Stats = new NPC.NPCStats { Strength = 5, Defense = 8, Intelligence = 14, Agility = 9 },
                IsActive = true
            },

            new NPC
            {
                Id = "671000000000000000000303",
                Name = "Guardián de Memoria",
                Role = "Protector de Archivos Liberados",
                FactionId = "671000000000000000000003",
                LevelId = null,
                PositionX = 900,
                PositionY = 260,
                DialogueList = new List<string>
                {
                    "Cada memoria salvada es una victoria.",
                    "La verdad pesa, pero también ilumina."
                },
                Stats = new NPC.NPCStats { Strength = 11, Defense = 12, Intelligence = 9, Agility = 8 },
                IsActive = true
            }
        };

        // Insertar
        await npcsCollection.InsertManyAsync(npcs);
        Console.WriteLine($" {npcs.Count} NPCs creados con éxito");

        // Verificar
        var verifyCount = await npcsCollection.CountDocumentsAsync(_ => true);
        Console.WriteLine($" Verificación: {verifyCount} NPCs en la base de datos");
    }
    catch (Exception ex)
    {
        Console.WriteLine($" Error creando NPCs: {ex.Message}");
        Console.WriteLine($"   Stack: {ex.StackTrace}");
    }
}


        private async Task AssignNPCsToLevelsAsync()
        {
            try
            {
                var dbService = new MongoDbService();
                var npcsCollection = dbService.GetCollection<NPC>("npcs");
                
                var allNpcs = await npcsCollection.Find(_ => true).ToListAsync();
                
                if (allNpcs.Count == 0)
                {
                    Console.WriteLine(" No hay NPCs para asignar");
                    return;
                }

                Console.WriteLine($" NPCs disponibles para asignar: {allNpcs.Count}");
                foreach (var npc in allNpcs)
                {
                    Console.WriteLine($"   - {npc.Name} (ID: {npc.Id})");
                }

                var levels = await _levelRepo.GetAllAsync();
                Console.WriteLine($" Niveles disponibles: {levels.Count}");
                
                // Asignar NPCs a niveles específicos
                // Asignación correcta basada en IDs, no índices
                var npcAssignments = new Dictionary<int, List<string>>
                {
                    { 1, new List<string> { "671000000000000000000101" } }, // Archivero
                    { 2, new List<string> { "671000000000000000000102" } }, // Custodio Alfa
                    { 3, new List<string> { "671000000000000000000103" } }, // Reportero Fantasma
                    { 4, new List<string> { "671000000000000000000201" } }, // Decano Villanueva
                    { 5, new List<string> { "671000000000000000000202" } }, // IA Centinela R-07
                    { 6, new List<string> { "671000000000000000000203" } }, // Analista Fractal
                    { 7, new List<string> { "671000000000000000000301" } }, // Noa Espectra
                };

                
                foreach (var level in levels)
                {
                    if (npcAssignments.TryGetValue(level.OrderNumber, out var npcIds))
                    {
                        level.NPCIds = npcIds;

                        bool updated = await _levelRepo.UpdateAsync(level.Id, level);

                        if (updated)
                        {
                            Console.WriteLine($" NPCs asignados al nivel {level.OrderNumber}: {string.Join(", ", npcIds)}");
                        }
                        else
                        {
                            Console.WriteLine($" No se pudo actualizar el nivel {level.OrderNumber}");
                        }
                    }
                    else
                    {
                        Console.WriteLine($" Nivel {level.OrderNumber} sin NPC asignado");
                    }
                }

                
                // Verificar la asignación
                Console.WriteLine("\n Verificando asignación de NPCs:");
                var verifyLevels = await _levelRepo.GetAllAsync();
                foreach (var level in verifyLevels)
                {
                    Console.WriteLine($"   Nivel {level.OrderNumber}: {level.NPCIds?.Count ?? 0} NPCs, {level.NPCs?.Count ?? 0} NPCs cargados");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error asignando NPCs: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
            }
        }
    }
}