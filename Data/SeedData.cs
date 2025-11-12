using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using MongoDB.Driver;

namespace GameAletheiaCross.Data
{
    public class SeedData
    {
        private readonly MongoDbService _dbService;

        public SeedData()
        {
            _dbService = new MongoDbService();
        }

        public async Task SeedAllAsync()
        {
            Console.WriteLine("🌱 Iniciando seed de datos...");
            
            await SeedFactionsAsync();
            await SeedNPCsAsync();
            await SeedLevelsAsync();
            await SeedPuzzlesAsync();
            
            Console.WriteLine("✅ Seed completado exitosamente!");
        }

        private async Task SeedFactionsAsync()
        {
            var factionsCollection = _dbService.GetCollection<Faction>("factions");
            var count = await factionsCollection.CountDocumentsAsync(_ => true);
            
            if (count > 0)
            {
                Console.WriteLine("⚠️ Las facciones ya existen, saltando...");
                return;
            }

            var factions = new List<Faction>
            {
                new Faction
                {
                    Name = "Hackers Éticos",
                    Type = "Hacking",
                    Leader = "Ghost",
                    Description = "Defensores de la libertad digital y la privacidad",
                    ThemeColor = "#00FF00"
                },
                new Faction
                {
                    Name = "Guardianes Corporativos",
                    Type = "Security",
                    Leader = "Director Smith",
                    Description = "Protectores del orden y la seguridad corporativa",
                    ThemeColor = "#0066FF"
                },
                new Faction
                {
                    Name = "Rebeldes Anónimos",
                    Type = "Rebellion",
                    Leader = "Phoenix",
                    Description = "Luchadores por la transparencia y contra la opresión",
                    ThemeColor = "#FF0066"
                }
            };

            await factionsCollection.InsertManyAsync(factions);
            Console.WriteLine($"✓ {factions.Count} facciones creadas");
        }

        private async Task SeedNPCsAsync()
        {
            var npcsCollection = _dbService.GetCollection<NPC>("npcs");
            var count = await npcsCollection.CountDocumentsAsync(_ => true);
            
            if (count > 0)
            {
                Console.WriteLine("⚠️ Los NPCs ya existen, saltando...");
                return;
            }

            var npcs = new List<NPC>
            {
                new NPC
                {
                    Name = "Guía Digital",
                    Role = "Tutorial",
                    FactionId = "",
                    PositionX = 300,
                    PositionY = 400,
                    Dialogue = "Bienvenido a Aletheia. Este es un mundo digital donde la verdad es relativa. Usa las flechas para moverte y ESPACIO para interactuar.",
                    IsActive = true
                },
                new NPC
                {
                    Name = "Hacker Misterioso",
                    Role = "Quest Giver",
                    FactionId = "",
                    PositionX = 500,
                    PositionY = 400,
                    Dialogue = "La terminal está bloqueada. Solo alguien con conocimientos de programación puede desbloquearla. ¿Te atreves?",
                    IsActive = true
                },
                new NPC
                {
                    Name = "Comerciante de Información",
                    Role = "Shop",
                    FactionId = "",
                    PositionX = 600,
                    PositionY = 400,
                    Dialogue = "Vendo pistas y conocimiento. Cada decisión que tomes afectará tu camino en este mundo.",
                    IsActive = true
                }
            };

            await npcsCollection.InsertManyAsync(npcs);
            Console.WriteLine($"✓ {npcs.Count} NPCs creados");
        }

private async Task SeedLevelsAsync()
{
    var levelRepo = new LevelRepository(_dbService);
    var npcsCollection = _dbService.GetCollection<NPC>("npcs");

    var existingLevels = await levelRepo.GetAllAsync();
    if (existingLevels.Count > 0)
    {
        Console.WriteLine("⚠️ Los niveles ya existen, saltando...");
        return;
    }

    var allNpcs = await npcsCollection.Find(_ => true).ToListAsync();
    var guiaId = allNpcs.Find(n => n.Name == "Guía Digital")?.Id ?? "";
    var hackerId = allNpcs.Find(n => n.Name == "Hacker Misterioso")?.Id ?? "";
    var comercianteId = allNpcs.Find(n => n.Name == "Comerciante de Información")?.Id ?? "";

    var levels = new List<Level>
    {
        new Level
        {
            Name = "El Despertar",
            Type = "Tutorial",
            Difficulty = 1,
            OrderNumber = 1,
            Description = "Despierta en un mundo digital desconocido. Aprende a moverte, saltar e interactuar con tu entorno.",
            TimeLimit = 300,
            Background = "cyber_city_start.png",
            Platforms = new List<Level.Platform>
            {
                new Level.Platform { X = 0, Y = 500, Width = 1280, Height = 30, IsSolid = true },
                new Level.Platform { X = 200, Y = 420, Width = 100, Height = 20, IsSolid = true },
                new Level.Platform { X = 400, Y = 350, Width = 100, Height = 20, IsSolid = true },
                new Level.Platform { X = 650, Y = 280, Width = 120, Height = 20, IsSolid = true }
            },
            Enemies = new List<Level.Enemy>(),
            NPCIds = new List<string> { guiaId }
        },
        new Level
        {
            Name = "La Primera Barrera",
            Type = "Puzzle",
            Difficulty = 2,
            OrderNumber = 2,
            Description = "Una terminal bloquea tu avance. Demuestra tus habilidades de programación para seguir adelante.",
            TimeLimit = 480,
            Background = "data_terminal_room.png",
            Platforms = new List<Level.Platform>
            {
                new Level.Platform { X = 0, Y = 500, Width = 1280, Height = 30, IsSolid = true },
                new Level.Platform { X = 250, Y = 420, Width = 100, Height = 20, IsSolid = true },
                new Level.Platform { X = 450, Y = 360, Width = 100, Height = 20, IsSolid = true },
                new Level.Platform { X = 650, Y = 300, Width = 100, Height = 20, IsSolid = true }
            },
            Enemies = new List<Level.Enemy>
            {
                new Level.Enemy { X = 600, Y = 470, Type = "FirewallBot", PatrolRoute = new List<string>{ "left", "right" } }
            },
            NPCIds = new List<string> { hackerId }
        },
        new Level
        {
            Name = "Redline Data Vault",
            Type = "Corporativo",
            Difficulty = 3,
            OrderNumber = 3,
            Description = "Has hackeado la red de Redline Corporation. Esquiva los drones de seguridad y llega al núcleo.",
            TimeLimit = 600,
            Background = "corporate_vault.png",
            Platforms = new List<Level.Platform>
            {
                new Level.Platform { X = 0, Y = 550, Width = 1280, Height = 30, IsSolid = true },
                new Level.Platform { X = 300, Y = 460, Width = 100, Height = 20, IsSolid = true },
                new Level.Platform { X = 500, Y = 380, Width = 100, Height = 20, IsSolid = true },
                new Level.Platform { X = 750, Y = 300, Width = 120, Height = 20, IsSolid = true }
            },
            Enemies = new List<Level.Enemy>
            {
                new Level.Enemy { X = 400, Y = 500, Type = "Drone", PatrolRoute = new List<string>{ "left", "right" } },
                new Level.Enemy { X = 700, Y = 480, Type = "Camera", PatrolRoute = new List<string>{ "scan" } }
            },
            NPCIds = new List<string> { comercianteId }
        },
        new Level
        {
            Name = "El Laberinto de Datos",
            Type = "Challenge",
            Difficulty = 4,
            OrderNumber = 4,
            Description = "Los datos fluyen como laberintos infinitos. Evita las trampas y encuentra la salida.",
            TimeLimit = 900,
            Background = "data_maze.png",
            Platforms = new List<Level.Platform>
            {
                new Level.Platform { X = 0, Y = 550, Width = 1280, Height = 30, IsSolid = true },
                new Level.Platform { X = 150, Y = 480, Width = 80, Height = 20, IsSolid = true },
                new Level.Platform { X = 300, Y = 410, Width = 80, Height = 20, IsSolid = true },
                new Level.Platform { X = 450, Y = 340, Width = 80, Height = 20, IsSolid = true },
                new Level.Platform { X = 600, Y = 280, Width = 80, Height = 20, IsSolid = true },
                new Level.Platform { X = 800, Y = 220, Width = 80, Height = 20, IsSolid = true }
            },
            Enemies = new List<Level.Enemy>
            {
                new Level.Enemy { X = 350, Y = 500, Type = "DataBug", PatrolRoute = new List<string>{ "left", "right" } },
                new Level.Enemy { X = 750, Y = 480, Type = "Glitch", PatrolRoute = new List<string>{ "dash", "hover" } }
            },
            NPCIds = new List<string>()
        },
        new Level
        {
            Name = "El Núcleo de Aletheia",
            Type = "Final",
            Difficulty = 5,
            OrderNumber = 5,
            Description = "Llegaste al núcleo. Las inteligencias artificiales de Aletheia te pondrán a prueba.",
            TimeLimit = 1200,
            Background = "aletheia_core.png",
            Platforms = new List<Level.Platform>
            {
                new Level.Platform { X = 0, Y = 550, Width = 1280, Height = 30, IsSolid = true },
                new Level.Platform { X = 350, Y = 480, Width = 150, Height = 20, IsSolid = true },
                new Level.Platform { X = 600, Y = 420, Width = 150, Height = 20, IsSolid = true },
                new Level.Platform { X = 850, Y = 360, Width = 150, Height = 20, IsSolid = true }
            },
            Enemies = new List<Level.Enemy>
            {
                new Level.Enemy { X = 500, Y = 500, Type = "Firewall", PatrolRoute = new List<string>{ "hover", "attack" } },
                new Level.Enemy { X = 800, Y = 500, Type = "AISentinel", PatrolRoute = new List<string>{ "dash", "idle" } }
            },
            NPCIds = new List<string>()
        }
    };

    foreach (var level in levels)
    {
        await levelRepo.CreateAsync(level);
    }

    Console.WriteLine($"✓ {levels.Count} niveles creados");
}


        private async Task SeedPuzzlesAsync()
        {
            var puzzleRepo = new PuzzleRepository(_dbService);
            var levelRepo = new LevelRepository(_dbService);
            
            var existingPuzzles = await puzzleRepo.GetAllAsync();
            if (existingPuzzles.Count > 0)
            {
                Console.WriteLine("⚠️ Los puzzles ya existen, saltando...");
                return;
            }

            var level2 = await levelRepo.GetByOrderNumberAsync(2);
            if (level2 == null)
            {
                Console.WriteLine("✗ No se pudo obtener el nivel 2");
                return;
            }

            var puzzles = new List<Puzzle>
            {
                new Puzzle
                {
                    Name = "Hola Mundo Digital",
                    Type = "Basic",
                    Description = "Tu primera misión: Escribe un programa que imprima 'Aletheia' en la consola.",
                    ExpectedOutput = "Aletheia",
                    StarterCode = @"public class Main {
    public static void main(String[] args) {
        // Escribe tu código aquí
        
    }
}",
                    LevelId = level2.Id,
                    Difficulty = 1,
                    Hints = new List<string>
                    {
                        "Usa System.out.println() para imprimir en la consola",
                        "Recuerda usar comillas dobles para el texto: \"Aletheia\"",
                        "No olvides el punto y coma al final de cada instrucción"
                    },
                    Points = 100
                }
            };

            foreach (var puzzle in puzzles)
            {
                await puzzleRepo.CreateAsync(puzzle);
            }
            
            Console.WriteLine($"✓ {puzzles.Count} puzzles creados");
        }
    }
}