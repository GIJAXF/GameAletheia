using ReactiveUI;
using System;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;

namespace GameAletheiaCross.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly string _playerId;
        private readonly string _playerName;
        
        private Player _player;
        private Level _currentLevel;
        private readonly PhysicsEngine _physics;
        private readonly CollisionManager _collision;
        private readonly LevelManager _levelManager;
        private readonly NPCInteractionManager _npcManager;
        private readonly PuzzleService _puzzleService;
        
        private CancellationTokenSource _gameLoopCts;
        private bool _isRunning;
        
        private bool _keyLeft, _keyRight, _keyUp, _keySpace;
        private bool _spacePressed = false;
        
        private const float PLAYER_SPEED = 5f;
        private const float JUMP_FORCE = -15f;
        private const float EXIT_DETECTION_DISTANCE = 80f;
        
        private string _levelInfo = "";
        private string _interactionHint = "";
        private string _statusMessage = "";
        private bool _isPaused = false;
        private bool _levelRequiresPuzzle = false;
        private bool _puzzlesCompleted = false;
        private bool _isCompletingLevel = false;
        
        public GameViewModel(Action<ViewModelBase> navigate, string playerId, string playerName)
        {
            _navigate = navigate;
            _playerId = playerId;
            _playerName = playerName;
            
            var dbService = new MongoDbService();
            _physics = new PhysicsEngine();
            _collision = new CollisionManager();
            _levelManager = new LevelManager(dbService);
            _npcManager = new NPCInteractionManager();
            _puzzleService = new PuzzleService(dbService);
            
            OpenTerminalCommand = ReactiveCommand.Create(OnOpenTerminal);
            PauseCommand = ReactiveCommand.Create(OnPause);
            
            InitializeGame();
        }
        
        // ============= PROPIEDADES PÚBLICAS =============
        
        public Player Player
        {
            get => _player;
            set => this.RaiseAndSetIfChanged(ref _player, value);
        }
        
        public Level CurrentLevel
        {
            get => _currentLevel;
            set => this.RaiseAndSetIfChanged(ref _currentLevel, value);
        }
        
        public string LevelInfo
        {
            get => _levelInfo;
            set => this.RaiseAndSetIfChanged(ref _levelInfo, value);
        }
        
        public string InteractionHint
        {
            get => _interactionHint;
            set => this.RaiseAndSetIfChanged(ref _interactionHint, value);
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }
        
        public bool KeyLeft
        {
            get => _keyLeft;
            set => this.RaiseAndSetIfChanged(ref _keyLeft, value);
        }
        
        public bool KeyRight
        {
            get => _keyRight;
            set => this.RaiseAndSetIfChanged(ref _keyRight, value);
        }
        
        public bool KeyUp
        {
            get => _keyUp;
            set => this.RaiseAndSetIfChanged(ref _keyUp, value);
        }
        
        public bool KeySpace
        {
            get => _keySpace;
            set
            {
                this.RaiseAndSetIfChanged(ref _keySpace, value);
                if (value && !_spacePressed && !_isPaused)
                {
                    _spacePressed = true;
                    CheckInteractions();
                }
                else if (!value)
                {
                    _spacePressed = false;
                }
            }
        }
        
        public ReactiveCommand<Unit, Unit> OpenTerminalCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }
        
        // ============= MÉTODOS PRIVADOS =============
        
        private async void InitializeGame()
        {
            try
            {
                Console.WriteLine("🎮 Inicializando juego...");
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                
                Player = await playerRepo.GetByIdAsync(_playerId);
                if (Player == null)
                {
                    Console.WriteLine(" Jugador no encontrado");
                    StatusMessage = "Error: Jugador no encontrado";
                    return;
                }
                
                Console.WriteLine($" Jugador cargado: {Player.Name} (Nivel {Player.CurrentLevel})");
                
                await LoadCurrentLevel();
                StartGameLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error iniciando juego: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }
        
        private async Task LoadCurrentLevel()
        {
            try
            {
                Console.WriteLine($"📂 Cargando nivel para jugador en nivel {Player.CurrentLevel}...");
                
                CurrentLevel = await _levelManager.GetCurrentLevelAsync(_playerId);
                
                if (CurrentLevel == null)
                {
                    Console.WriteLine(" Nivel no encontrado");
                    StatusMessage = "Error: Nivel no encontrado";
                    return;
                }
                
                // Verificar si este nivel requiere completar puzzles
                await CheckLevelPuzzleRequirements();
                
                // Reiniciar posición del jugador
                ResetPlayerPosition();
                
                LevelInfo = $"Nivel {Player.CurrentLevel}: {CurrentLevel.Name}";
                
                if (_levelRequiresPuzzle && !_puzzlesCompleted)
                {
                    StatusMessage = $"{CurrentLevel.Description}\n⚠️ Debes resolver el puzzle de programación. Presiona T.";
                }
                else
                {
                    StatusMessage = CurrentLevel.Description;
                }
                
                Console.WriteLine($" {LevelInfo}");
                Console.WriteLine($"   Plataformas: {CurrentLevel.Platforms?.Count ?? 0}");
                Console.WriteLine($"   NPCs: {CurrentLevel.NPCs?.Count ?? 0}");
                Console.WriteLine($"   Requiere puzzles: {_levelRequiresPuzzle}");
                Console.WriteLine($"   Puzzles completados: {_puzzlesCompleted}");
                
                this.RaisePropertyChanged(nameof(CurrentLevel));
                this.RaisePropertyChanged(nameof(Player));
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error cargando nivel: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private async Task CheckLevelPuzzleRequirements()
        {
            var puzzle = await _puzzleService.GetPuzzleForLevelAsync(CurrentLevel.Id);
            _levelRequiresPuzzle = puzzle != null;
            _puzzlesCompleted = puzzle?.IsCompleted ?? true;

            Console.WriteLine($"🧩 Puzzle requerido: {_levelRequiresPuzzle}, Completado: {_puzzlesCompleted}");
        }

        private void ResetPlayerPosition()
        {
            Player.Position.X = 100;
            Player.Position.Y = 400;
            Player.Velocity.X = 0;
            Player.Velocity.Y = 0;
            Player.IsJumping = false;
            Console.WriteLine("🔄 Posición del jugador reiniciada");
        }
        
        private void StartGameLoop()
        {
            _gameLoopCts = new CancellationTokenSource();
            _isRunning = true;
            _isPaused = false;
            _isCompletingLevel = false;
            
            Console.WriteLine("▶️ Game loop iniciado");
            
            Task.Run(async () =>
            {
                while (_isRunning && !_gameLoopCts.Token.IsCancellationRequested)
                {
                    GameLoop();
                    await Task.Delay(16); // ~60 FPS
                }
            }, _gameLoopCts.Token);
        }
        
        private void GameLoop()
        {
            if (Player == null || CurrentLevel == null || _isPaused) return;
            
            UpdatePlayerMovement();
            _physics.ApplyGravity(Player);
            _physics.UpdatePosition(Player);
            _collision.CheckPlatformCollisions(Player, CurrentLevel.Platforms);
            
            // Límites del mundo
            if (Player.Position.X < 0) Player.Position.X = 0;
            if (Player.Position.X > 1240) Player.Position.X = 1240;
            
            // Caída fuera del mapa
            if (Player.Position.Y > 800)
            {
                ResetPlayerPosition();
            }

            CheckNearbyInteractions();
            CheckLevelExit();
            
            this.RaisePropertyChanged(nameof(Player));
        }
        
        private void UpdatePlayerMovement()
        {
            if (KeyLeft)
            {
                Player.Velocity.X = -PLAYER_SPEED;
                Player.IsFacingRight = false;
            }
            else if (KeyRight)
            {
                Player.Velocity.X = PLAYER_SPEED;
                Player.IsFacingRight = true;
            }
            else
            {
                Player.Velocity.X = 0;
            }
            
            if (KeyUp && !Player.IsJumping)
            {
                Player.Velocity.Y = JUMP_FORCE;
                Player.IsJumping = true;
            }
        }

        private void CheckNearbyInteractions()
        {
            if (CurrentLevel?.NPCs == null || CurrentLevel.NPCs.Count == 0)
            {
                InteractionHint = "";
                return;
            }

            var nearestNPC = _npcManager.FindNearestNPC(Player, CurrentLevel.NPCs);
            
            if (nearestNPC != null)
            {
                InteractionHint = $"💬 Presiona ESPACIO para hablar con {nearestNPC.Name}";
            }
            else
            {
                InteractionHint = "";
            }
        }

        private void CheckInteractions()
        {
            if (CurrentLevel?.NPCs == null || CurrentLevel.NPCs.Count == 0)
            {
                return;
            }

            var nearestNPC = _npcManager.FindNearestNPC(Player, CurrentLevel.NPCs);
            
            if (nearestNPC != null)
            {
                Console.WriteLine($"💬 Hablando con: {nearestNPC.Name}");
                _npcManager.MarkAsInteracted(nearestNPC);
                
                PauseGameLoop();
                Dispatcher.UIThread.Post(() =>
                {
                    _navigate(new DialogueViewModel(_navigate, nearestNPC, this));
                });
            }
        }

        private async void CheckLevelExit()
        {
            if (_isCompletingLevel) return; // Evitar múltiples llamadas
            
            // Verificar si el jugador está cerca del portal de salida
            float exitX = 700f;
            float exitY = 420f;
            
            float distance = (float)Math.Sqrt(
                Math.Pow(Player.Position.X - exitX, 2) + 
                Math.Pow(Player.Position.Y - exitY, 2)
            );

            if (distance < EXIT_DETECTION_DISTANCE)
            {
                Console.WriteLine($"🚪 Jugador cerca del portal (distancia: {distance:F1})");
                
                // Verificar si se pueden completar los puzzles antes de avanzar
                if (_levelRequiresPuzzle)
                {
                    // Verificar de nuevo el estado de los puzzles
                    _puzzlesCompleted = await _puzzleService.AreLevelPuzzlesCompletedAsync(CurrentLevel.Id);
                    
                    Console.WriteLine($"🧩 Verificación de puzzles: Requerido={_levelRequiresPuzzle}, Completado={_puzzlesCompleted}");
                    
                    if (!_puzzlesCompleted)
                    {
                        if (string.IsNullOrEmpty(StatusMessage) || !StatusMessage.Contains("🔒"))
                        {
                            StatusMessage = "🔒 Debes resolver el puzzle de programación antes de continuar. Presiona T para abrir la terminal.";
                        }
                        return;
                    }
                }

                Console.WriteLine("🎯 ¡Condiciones cumplidas! Completando nivel...");
                _isCompletingLevel = true;
                await OnLevelComplete();
            }
        }

        private async Task OnLevelComplete()
        {
            PauseGameLoop();
            
            Console.WriteLine("🎉 ¡Nivel completado!");
            StatusMessage = "🎉 ¡Nivel completado!";
            
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                
                // Calcular puntos por completar nivel
                int levelPoints = 100 * CurrentLevel.Difficulty;
                Player.TotalScore += levelPoints;
                await playerRepo.UpdateAsync(_playerId, Player);
                
                Console.WriteLine($" +{levelPoints} puntos. Score total: {Player.TotalScore}");
                StatusMessage = $"✓ +{levelPoints} puntos. Score total: {Player.TotalScore}";
                
                await Task.Delay(2000);
                
                // Avanzar al siguiente nivel
                Console.WriteLine("⏭️ Intentando avanzar al siguiente nivel...");
                bool advanced = await _levelManager.AdvanceToNextLevelAsync(_playerId);
                
                if (advanced)
                {
                    Console.WriteLine(" Avanzado exitosamente");
                    StatusMessage = "✓ Avanzando al siguiente nivel...";
                    
                    // Recargar el jugador actualizado desde la BD
                    Player = await playerRepo.GetByIdAsync(_playerId);
                    Console.WriteLine($"🔄 Jugador recargado: Nivel actual = {Player.CurrentLevel}");
                    
                    await LoadCurrentLevel();
                    
                    await Task.Delay(1000);
                    _isCompletingLevel = false;
                    ResumeGameLoop();
                }
                else
                {
                    Console.WriteLine("🏆 Juego completado");
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        StatusMessage = "🏆 ¡¡HAS COMPLETADO EL JUEGO!!";
                        _navigate(new MainMenuViewModel(_navigate));
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error completando nivel: {ex.Message}");
                Console.WriteLine($"   Stack: {ex.StackTrace}");
                StatusMessage = $"Error: {ex.Message}";
                _isCompletingLevel = false;
                ResumeGameLoop();
            }
        }
        
        private void OnOpenTerminal()
        {
            if (_isPaused) return;
            
            PauseGameLoop();
            Console.WriteLine("💻 Terminal abierta");
            
            Dispatcher.UIThread.Post(() =>
            {
                _navigate(new TerminalViewModel(_navigate, _playerId, CurrentLevel.Id, this));
            });
        }
        
        private async void OnPause()
        {
            PauseGameLoop();
            
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                await playerRepo.UpdateAsync(_playerId, Player);
                Console.WriteLine("✓ Progreso guardado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error guardando: {ex.Message}");
            }
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Console.WriteLine("⏸️ Juego pausado - Volviendo al menú");
                _navigate(new MainMenuViewModel(_navigate));
            });
        }

        // ============= MÉTODOS PÚBLICOS =============
        
        public void OnPauseRequested()
        {
            Console.WriteLine("⏸️ Pausa solicitada (ESC presionado)");
            OnPause();
        }

        public void OnToggleTerminal()
        {
            Console.WriteLine("💻 Alternar terminal (T presionada)");
            if (!_isPaused)
            {
                OnOpenTerminal();
            }
        }

        public async Task OnPuzzleCompletedAsync(string puzzleId)
        {
            Console.WriteLine($"🧩 Puzzle {puzzleId} completado - Actualizando estado...");
            _puzzlesCompleted = await _puzzleService.AreLevelPuzzlesCompletedAsync(CurrentLevel.Id);
            
            Console.WriteLine($" Estado actualizado: Puzzles completados = {_puzzlesCompleted}");
            
            if (_puzzlesCompleted)
            {
                StatusMessage = " ¡Puzzles completados! Dirígete al portal verde para continuar.";
            }
        }

        public void ReturnFromSubView()
        {
            Dispatcher.UIThread.Post(() =>
            {
                Console.WriteLine("🔙 Regresando de subvista");
                _isPaused = false;
                ResumeGameLoop();
                this.RaisePropertyChanged(nameof(Player));
            });
        }
        
        public void ResumeGameLoop()
        {
            Console.WriteLine("▶️ Reanudando game loop");
            if (!_isRunning)
            {
                StartGameLoop();
            }
            else
            {
                _isPaused = false;
            }
        }
        
        private void PauseGameLoop()
        {
            Console.WriteLine("⏸️ Pausando game loop");
            _isPaused = true;
        }

        private void StopGameLoop()
        {
            Console.WriteLine("⏹️ Deteniendo game loop");
            _isRunning = false;
            _gameLoopCts?.Cancel();
        }
    }
}