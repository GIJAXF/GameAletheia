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
        
        private CancellationTokenSource _gameLoopCts;
        private bool _isRunning;
        
        private bool _keyLeft, _keyRight, _keyUp, _keySpace;
        private bool _spacePressed = false;
        
        private const float PLAYER_SPEED = 5f;
        private const float JUMP_FORCE = -15f;
        private const float EXIT_DETECTION_DISTANCE = 50f;
        
        private string _levelInfo = "";
        private string _interactionHint = "";
        private bool _isPaused = false;
        
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
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                
                Player = await playerRepo.GetByIdAsync(_playerId);
                if (Player == null)
                {
                    Console.WriteLine("✗ Jugador no encontrado");
                    return;
                }
                
                Console.WriteLine($"✓ Jugador cargado: {Player.Name}");
                
                await LoadCurrentLevel();
                StartGameLoop();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error iniciando juego: {ex.Message}");
            }
        }
        
        private async Task LoadCurrentLevel()
        {
            try
            {
                CurrentLevel = await _levelManager.GetCurrentLevelAsync(_playerId);
                
                if (CurrentLevel == null)
                {
                    Console.WriteLine("✗ Nivel no encontrado");
                    return;
                }
                
                // Reiniciar posición del jugador
                Player.Position.X = 100;
                Player.Position.Y = 400;
                Player.Velocity.X = 0;
                Player.Velocity.Y = 0;
                Player.IsJumping = false;
                
                LevelInfo = $"Nivel {Player.CurrentLevel}: {CurrentLevel.Name}";
                Console.WriteLine($"✓ {LevelInfo}");
                Console.WriteLine($"✓ Plataformas: {CurrentLevel.Platforms?.Count ?? 0}");
                Console.WriteLine($"✓ NPCs: {CurrentLevel.NPCs?.Count ?? 0}");
                
                this.RaisePropertyChanged(nameof(CurrentLevel));
                this.RaisePropertyChanged(nameof(Player));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error cargando nivel: {ex.Message}");
            }
        }
        
        private void StartGameLoop()
        {
            _gameLoopCts = new CancellationTokenSource();
            _isRunning = true;
            _isPaused = false;
            
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
                Player.Position.X = 100;
                Player.Position.Y = 400;
                Player.Velocity.Y = 0;
                Player.Velocity.X = 0;
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
                InteractionHint = $"Presiona ESPACIO para hablar con {nearestNPC.Name}";
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
                Console.WriteLine("⚠️ No hay NPCs en este nivel");
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
            else
            {
                Console.WriteLine("⚠️ No hay NPCs cercanos");
            }
        }

        private void CheckLevelExit()
        {
            float exitX = 700f;
            float distance = Math.Abs(Player.Position.X - exitX);

            if (distance < EXIT_DETECTION_DISTANCE)
            {
                Console.WriteLine("🎯 ¡Jugador llegó a la salida!");
                OnLevelComplete();
            }
        }

        private async void OnLevelComplete()
        {
            PauseGameLoop();
            
            Console.WriteLine("🎉 ¡Nivel completado!");
            
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                
                // Sumar puntos por completar nivel
                Player.TotalScore += 100;
                await playerRepo.UpdateAsync(_playerId, Player);
                
                Console.WriteLine($"✓ +100 puntos. Score total: {Player.TotalScore}");
                
                bool canAdvance = await _levelManager.CanAdvanceAsync(_playerId, CurrentLevel.Id);
                
                if (canAdvance)
                {
                    bool advanced = await _levelManager.AdvanceToNextLevelAsync(_playerId);
                    
                    if (advanced)
                    {
                        Console.WriteLine("✓ Avanzando al siguiente nivel...");
                        await LoadCurrentLevel();
                        
                        await Task.Delay(1000);
                        ResumeGameLoop();
                    }
                    else
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Console.WriteLine("🏆 ¡¡HAS COMPLETADO EL JUEGO!!");
                            _navigate(new MainMenuViewModel(_navigate));
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error completando nivel: {ex.Message}");
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

        public void ReturnFromSubView()
        {
            Dispatcher.UIThread.Post(() =>
            {
                _isPaused = false;
                ResumeGameLoop();
                this.RaisePropertyChanged(nameof(Player));
            });
        }
        
        public void ResumeGameLoop()
        {
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
            _isPaused = true;
        }

        private void StopGameLoop()
        {
            _isRunning = false;
            _gameLoopCts?.Cancel();
        }
    }
}