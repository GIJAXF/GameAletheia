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
                Console.WriteLine($"⚠️ Ya existen {existingCount} niveles en la base de datos. No se generarán nuevos.");
                return;
            }

            // 🧩 Lista de niveles base
            var levels = new List<Level>
            {
                new Level { OrderNumber = 1, Name = "El Inicio", Description = "Aprende los controles básicos.", Background = "forest", Difficulty = 1 },
                new Level { OrderNumber = 2, Name = "Ruinas Perdidas", Description = "Encuentra el primer artefacto.", Background = "ruins", Difficulty = 1 },
                new Level { OrderNumber = 3, Name = "Ciudad del Silencio", Description = "Evita las trampas de la ciudad.", Background = "city", Difficulty = 2 },
                new Level { OrderNumber = 4, Name = "Laberinto Digital", Description = "Resuelve el primer puzzle lógico.", Background = "digital", Difficulty = 2 },
                new Level { OrderNumber = 5, Name = "Santuario de Datos", Description = "Defiende el núcleo resolviendo un código.", Background = "temple", Difficulty = 3 },
                new Level { OrderNumber = 6, Name = "Redline", Description = "Enfrenta a los agentes de la corporación.", Background = "cyber", Difficulty = 3 },
                new Level { OrderNumber = 7, Name = "El Archivo", Description = "Descubre la verdad detrás de Aletheia.", Background = "archive", Difficulty = 3 },
            };

            // 🔹 Insertar niveles
            foreach (var level in levels)
            {
                await _levelRepo.CreateAsync(level);
                Console.WriteLine($"✓ Nivel creado: {level.Name} (Orden: {level.OrderNumber})");
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
                Difficulty = difficulty
            };

            await _levelRepo.CreateAsync(level);
            Console.WriteLine($"✓ Nivel personalizado creado: {level.Name}");

            return level;
        }
    }
}
