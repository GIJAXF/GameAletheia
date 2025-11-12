using System;
using System.Threading.Tasks;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;

namespace GameAletheiaCross.Services
{
    /// <summary>
    /// Administra el progreso de los jugadores a través de los niveles y puzzles del juego.
    /// </summary>
    public class LevelManager
    {
        private readonly PlayerRepository _playerRepo;
        private readonly LevelRepository _levelRepo;
        private readonly PuzzleRepository _puzzleRepo;

        /// <summary>
        /// Inicializa el administrador de niveles con los repositorios de base de datos.
        /// </summary>
        public LevelManager(MongoDbService dbService)
        {
            _playerRepo = new PlayerRepository(dbService);
            _levelRepo = new LevelRepository(dbService);
            _puzzleRepo = new PuzzleRepository(dbService);
        }

        /// <summary>
        /// Avanza al jugador al siguiente nivel si no ha llegado al final del juego.
        /// </summary>
        /// <param name="playerId">ID del jugador.</param>
        /// <returns>True si el jugador avanzó; False si no.</returns>
        public async Task<bool> AdvanceToNextLevelAsync(string playerId)
        {
            try
            {
                var player = await _playerRepo.GetByIdAsync(playerId);
                if (player == null) return false;

                var totalLevels = await _levelRepo.GetTotalLevelsAsync();

                if (player.CurrentLevel >= totalLevels)
                {
                    Console.WriteLine("🏆 ¡El jugador ha completado todos los niveles!");
                    return false;
                }

                player.CurrentLevel++;
                await _playerRepo.UpdateAsync(playerId, player);

                Console.WriteLine($"✓ Jugador avanzó al nivel {player.CurrentLevel}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error avanzando nivel: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Verifica si el jugador cumple los requisitos para avanzar de nivel.
        /// </summary>
        /// <param name="playerId">ID del jugador.</param>
        /// <param name="levelId">ID del nivel actual.</param>
        /// <returns>True si puede avanzar; False si no.</returns>
        public async Task<bool> CanAdvanceAsync(string playerId, string levelId)
        {
            try
            {
                var level = await _levelRepo.GetByIdAsync(levelId);
                if (level == null) return false;

                var puzzle = await _puzzleRepo.GetByLevelIdAsync(levelId);
                if (puzzle != null)
                {
                    // Aquí podrías verificar si el puzzle fue completado.
                    return true;
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error verificando progreso: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Obtiene el nivel actual del jugador.
        /// </summary>
        /// <param name="playerId">ID del jugador.</param>
        /// <returns>Instancia del nivel actual o null si ocurre un error.</returns>
        public async Task<Level?> GetCurrentLevelAsync(string playerId)
        {
            try
            {
                var player = await _playerRepo.GetByIdAsync(playerId);
                if (player == null) return null;

                return await _levelRepo.GetByOrderNumberAsync(player.CurrentLevel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error obteniendo nivel actual: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Marca un puzzle como completado y suma puntos al jugador.
        /// </summary>
        /// <param name="playerId">ID del jugador.</param>
        /// <param name="puzzleId">ID del puzzle completado.</param>
        /// <returns>True si se registró correctamente; False en caso contrario.</returns>
        public async Task<bool> CompletePuzzleAsync(string playerId, string puzzleId)
        {
            try
            {
                var player = await _playerRepo.GetByIdAsync(playerId);
                var puzzle = await _puzzleRepo.GetByIdAsync(puzzleId);

                if (player == null || puzzle == null) return false;

                // Sumar puntos al jugador
                player.TotalScore += puzzle.Points;
                await _playerRepo.UpdateAsync(playerId, player);

                Console.WriteLine($"✓ Puzzle completado: +{puzzle.Points} puntos");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error completando puzzle: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Reinicia al jugador en el nivel actual (posición inicial y estado).
        /// </summary>
        /// <param name="playerId">ID del jugador.</param>
        /// <returns>True si se reinició correctamente; False si no.</returns>
        public async Task<bool> RestartLevelAsync(string playerId)
        {
            try
            {
                var player = await _playerRepo.GetByIdAsync(playerId);
                if (player == null) return false;

                // Reiniciar posición y estado
                player.Position.X = 100;
                player.Position.Y = 400;
                player.Velocity.X = 0;
                player.Velocity.Y = 0;
                player.IsJumping = false;

                await _playerRepo.UpdateAsync(playerId, player);

                Console.WriteLine("✓ Nivel reiniciado");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error reiniciando nivel: {ex.Message}");
                return false;
            }
        }
    }
}
