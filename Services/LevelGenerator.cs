using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database.Repositories;

namespace GameAletheiaCross.Services
{
    /// <summary>
    /// Genera niveles base y puzzles asociados, registrándolos en la base de datos.
    /// </summary>
    public class LevelGenerator
    {
        private readonly LevelRepository _levelRepo;
        private readonly PuzzleRepository _puzzleRepo;

        public LevelGenerator(LevelRepository levelRepo, PuzzleRepository puzzleRepo)
        {
            _levelRepo = levelRepo;
            _puzzleRepo = puzzleRepo;
        }

        /// <summary>
        /// Genera y guarda una lista de niveles y puzzles predefinidos.
        /// </summary>
        public async Task GenerateDefaultLevelsAsync()
        {
            var existingCount = await _levelRepo.GetTotalLevelsAsync();
            if (existingCount > 0)
            {
                Console.WriteLine($"⚠️ Ya existen {existingCount} niveles en la base de datos. Eliminando para regenerar...");
                // 🗑️ Eliminamos los antiguos para regenerar con plataformas
                await DeleteAllLevelsAsync();
            }

            // 🧩 Lista de niveles base
            var levels = new List<Level>
            {
                new Level 
                { 
                    OrderNumber = 1, 
                    Name = "El Inicio", 
                    Description = "Aprende los controles básicos.", 
                    Background = "forest", 
                    Difficulty = 1,
                    Platforms = GeneratePlatformsForLevel(1),
                    NPCIds = new List<string>()
                },
                new Level 
                { 
                    OrderNumber = 2, 
                    Name = "Ruinas Perdidas", 
                    Description = "Encuentra el primer artefacto.", 
                    Background = "ruins", 
                    Difficulty = 1,
                    Platforms = GeneratePlatformsForLevel(2),
                    NPCIds = new List<string>()
                },
                new Level 
                { 
                    OrderNumber = 3, 
                    Name = "Ciudad del Silencio", 
                    Description = "Evita las trampas de la ciudad.", 
                    Background = "city", 
                    Difficulty = 2,
                    Platforms = GeneratePlatformsForLevel(3),
                    NPCIds = new List<string>()
                },
                new Level 
                { 
                    OrderNumber = 4, 
                    Name = "Laberinto Digital", 
                    Description = "Resuelve el primer puzzle lógico.", 
                    Background = "digital", 
                    Difficulty = 2,
                    Platforms = GeneratePlatformsForLevel(4),
                    NPCIds = new List<string>()
                },
                new Level 
                { 
                    OrderNumber = 5, 
                    Name = "Santuario de Datos", 
                    Description = "Defiende el núcleo resolviendo un código.", 
                    Background = "temple", 
                    Difficulty = 3,
                    Platforms = GeneratePlatformsForLevel(5),
                    NPCIds = new List<string>()
                },
                new Level 
                { 
                    OrderNumber = 6, 
                    Name = "Redline", 
                    Description = "Enfrenta a los agentes de la corporación.", 
                    Background = "cyber", 
                    Difficulty = 3,
                    Platforms = GeneratePlatformsForLevel(6),
                    NPCIds = new List<string>()
                },
                new Level 
                { 
                    OrderNumber = 7, 
                    Name = "El Archivo", 
                    Description = "Descubre la verdad detrás de Aletheia.", 
                    Background = "archive", 
                    Difficulty = 3,
                    Platforms = GeneratePlatformsForLevel(7),
                    NPCIds = new List<string>()
                },
            };

            // 🔹 Insertar niveles
            foreach (var level in levels)
            {
                await _levelRepo.CreateAsync(level);
                Console.WriteLine($"✓ Nivel creado: {level.Name} (Orden: {level.OrderNumber}, Plataformas: {level.Platforms.Count})");
            }

            // 🧠 Crear puzzles para niveles específicos
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

            Console.WriteLine("✅ Todos los niveles y puzzles base han sido generados correctamente.");
        }

        /// <summary>
        /// Genera plataformas específicas para cada nivel.
        /// </summary>
        private List<Level.Platform> GeneratePlatformsForLevel(int levelNumber)
        {
            var platforms = new List<Level.Platform>();

            // 🏗️ NIVEL 1 - El Inicio (Simple)
            if (levelNumber == 1)
            {
                // Piso principal
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                
                // Plataformas intermedias ascendentes
                platforms.Add(new Level.Platform { X = 200, Y = 520, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 450, Y = 450, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 380, Width = 150, Height = 20, IsSolid = true });
                
                // Salida al final
                platforms.Add(new Level.Platform { X = 1000, Y = 300, Width = 200, Height = 20, IsSolid = true });
            }

            // 🏗️ NIVEL 2 - Ruinas Perdidas
            else if (levelNumber == 2)
            {
                // Piso base
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                
                // Plataformas de ruinas (irregulares)
                platforms.Add(new Level.Platform { X = 100, Y = 500, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 350, Y = 450, Width = 180, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 580, Y = 400, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 850, Y = 350, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1050, Y = 300, Width = 200, Height = 25, IsSolid = true });
            }

            // 🏗️ NIVEL 3 - Ciudad del Silencio
            else if (levelNumber == 3)
            {
                // Piso base
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                
                // Estructura de ciudad
                platforms.Add(new Level.Platform { X = 50, Y = 500, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 480, Width = 100, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 450, Y = 420, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 380, Width = 120, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 900, Y = 350, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 300, Width = 130, Height = 20, IsSolid = true });
                
                // Plataforma trampa (no sólida - opcional)
                platforms.Add(new Level.Platform { X = 600, Y = 500, Width = 100, Height = 15, IsSolid = false });
            }

            // 🏗️ NIVEL 4 - Laberinto Digital
            else if (levelNumber == 4)
            {
                // Piso base
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                
                // Camino en zigzag
                platforms.Add(new Level.Platform { X = 50, Y = 500, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 300, Y = 450, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 150, Y = 400, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 400, Y = 350, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 300, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 500, Y = 250, Width = 150, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 800, Y = 200, Width = 200, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1000, Y = 300, Width = 200, Height = 20, IsSolid = true });
            }

            // 🏗️ NIVEL 5 - Santuario de Datos
            else if (levelNumber == 5)
            {
                // Piso base
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                
                // Estructura de templo
                platforms.Add(new Level.Platform { X = 100, Y = 500, Width = 200, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 350, Y = 430, Width = 250, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 700, Y = 380, Width = 250, Height = 25, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1000, Y = 300, Width = 200, Height = 25, IsSolid = true });
                
                // Núcleo (zona especial)
                platforms.Add(new Level.Platform { X = 640, Y = 200, Width = 100, Height = 20, IsSolid = true });
            }

            // 🏗️ NIVEL 6 - Redline
            else if (levelNumber == 6)
            {
                // Piso base
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                
                // Estructura cibernética irregular
                platforms.Add(new Level.Platform { X = 0, Y = 500, Width = 180, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 250, Y = 450, Width = 200, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 550, Y = 400, Width = 180, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 850, Y = 350, Width = 200, Height = 20, IsSolid = true });
                platforms.Add(new Level.Platform { X = 1100, Y = 280, Width = 180, Height = 20, IsSolid = true });
            }

            // 🏗️ NIVEL 7 - El Archivo
            else if (levelNumber == 7)
            {
                // Piso base
                platforms.Add(new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true });
                
                // Estructura de archivo compleja
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

        /// <summary>
        /// Elimina todos los niveles existentes para regenerarlos.
        /// </summary>
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

        /// <summary>
        /// Crea un nivel personalizado desde código o consola.
        /// </summary>
        public async Task<Level> CreateCustomLevelAsync(string name, string background, int difficulty, string description)
        {
            var total = await _levelRepo.GetTotalLevelsAsync();

            var level = new Level
            {
                OrderNumber = total + 1,
                Name = name,
                Description = description,
                Background = background,
                Difficulty = difficulty,
                Platforms = GeneratePlatformsForLevel(total + 1)
            };

            await _levelRepo.CreateAsync(level);
            Console.WriteLine($"✓ Nivel personalizado creado: {level.Name}");

            return level;
        }
    }
}