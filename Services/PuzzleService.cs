using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;

namespace GameAletheiaCross.Services
{
    /// <summary>
    /// Servicio para gestionar puzzles de programación
    /// </summary>
    public class PuzzleService
    {
        private readonly PuzzleRepository _puzzleRepo;
        private readonly PlayerRepository _playerRepo;

        public PuzzleService(MongoDbService dbService)
        {
            _puzzleRepo = new PuzzleRepository(dbService);
            _playerRepo = new PlayerRepository(dbService);
        }

        /// <summary>
        /// Obtiene el puzzle activo para un nivel específico
        /// </summary>
        public async Task<Puzzle?> GetPuzzleForLevelAsync(string levelId)
        {
            var puzzles = await _puzzleRepo.GetAllByLevelIdAsync(levelId);
            return puzzles.FirstOrDefault(p => !p.IsCompleted);
        }

        /// <summary>
        /// Valida la solución de un puzzle
        /// </summary>
        public async Task<PuzzleValidationResult> ValidateSolutionAsync(
            string playerId, 
            string puzzleId, 
            string code, 
            string output)
        {
            var result = new PuzzleValidationResult();
            
            try
            {
                var puzzle = await _puzzleRepo.GetByIdAsync(puzzleId);
                if (puzzle == null)
                {
                    result.IsValid = false;
                    result.Message = "Puzzle no encontrado";
                    return result;
                }

                // Validar salida
                var expectedOutput = puzzle.ExpectedOutput.Trim();
                var actualOutput = output.Trim();

                result.IsValid = string.Equals(expectedOutput, actualOutput, StringComparison.Ordinal);
                
                if (result.IsValid)
                {
                    // Marcar puzzle como completado
                    puzzle.IsCompleted = true;
                    await _puzzleRepo.UpdateAsync(puzzleId, puzzle);

                    // Otorgar puntos al jugador
                    var player = await _playerRepo.GetByIdAsync(playerId);
                    if (player != null)
                    {
                        player.TotalScore += puzzle.Points;
                        await _playerRepo.UpdateAsync(playerId, player);
                    }

                    result.PointsEarned = puzzle.Points;
                    result.Message = $"¡Correcto! Has ganado {puzzle.Points} puntos";
                }
                else
                {
                    result.Message = $"Incorrecto.\nEsperado: {expectedOutput}\nObtenido: {actualOutput}";
                }
            }
            catch (Exception ex)
            {
                result.IsValid = false;
                result.Message = $"Error validando: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// Obtiene la siguiente pista disponible para un puzzle
        /// </summary>
        public string GetNextHint(Puzzle puzzle, int currentHintIndex)
        {
            if (puzzle.Hints == null || puzzle.Hints.Count == 0)
                return "No hay pistas disponibles";

            if (currentHintIndex >= puzzle.Hints.Count)
                return "Ya has visto todas las pistas disponibles";

            return puzzle.Hints[currentHintIndex];
        }

        /// <summary>
        /// Verifica si todos los puzzles de un nivel están completados
        /// </summary>
        public async Task<bool> AreLevelPuzzlesCompletedAsync(string levelId)
        {
            var puzzles = await _puzzleRepo.GetAllByLevelIdAsync(levelId);
            return puzzles.All(p => p.IsCompleted);
        }

        /// <summary>
        /// Reinicia el progreso de puzzles para un nivel
        /// </summary>
        public async Task<bool> ResetLevelPuzzlesAsync(string levelId)
        {
            try
            {
                var puzzles = await _puzzleRepo.GetAllByLevelIdAsync(levelId);
                foreach (var puzzle in puzzles)
                {
                    puzzle.IsCompleted = false;
                    await _puzzleRepo.UpdateAsync(puzzle.Id, puzzle);
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reseteando puzzles: {ex.Message}");
                return false;
            }
        }
    }

    public class PuzzleValidationResult
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = "";
        public int PointsEarned { get; set; }
    }
}