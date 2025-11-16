using ReactiveUI;
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Tutorial interactivo para enseñar los controles básicos del juego.
    /// Se ejecuta después de crear el personaje y antes de seleccionar facción.
    /// </summary>
    public class TutorialViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly string _playerId;
        private readonly string _playerName;
        
        private Player _player;
        private readonly PhysicsEngine _physics;
        private readonly CollisionManager _collision;
        
        private CancellationTokenSource _gameLoopCts;
        private bool _isRunning;
        
        private bool _keyLeft, _keyRight, _keyUp;
        
        private const float PLAYER_SPEED = 5f;
        private const float JUMP_FORCE = -15f;
        
        // Estados del tutorial
        private int _currentStep = 0;
        private bool _hasMovedLeft = false;
        private bool _hasMovedRight = false;
        private bool _hasJumped = false;
        private bool _hasReachedEnd = false;
        
        private string _instructionText = "";
        private string _progressText = "";
        
        // Plataformas del tutorial
        private readonly List<Level.Platform> _tutorialPlatforms = new()
        {
            new Level.Platform { X = 0, Y = 600, Width = 1280, Height = 120, IsSolid = true }, // Piso
            new Level.Platform { X = 300, Y = 550, Width = 150, Height = 20, IsSolid = true },
            new Level.Platform { X = 500, Y = 500, Width = 150, Height = 20, IsSolid = true },
            new Level.Platform { X = 700, Y = 450, Width = 150, Height = 20, IsSolid = true },
            new Level.Platform { X = 900, Y = 400, Width = 200, Height = 20, IsSolid = true }
        };
        
        public TutorialViewModel(Action<ViewModelBase> navigate, string playerId, string playerName)
        {
            _navigate = navigate;
            _playerId = playerId;
            _playerName = playerName;
            
            _physics = new PhysicsEngine();
            _collision = new CollisionManager();
            
            ContinueCommand = ReactiveCommand.Create(OnContinue);
            
            InitializeTutorial();
        }
        
        // ============= PROPIEDADES PÚBLICAS =============
        
        public Player Player
        {
            get => _player;
            set => this.RaiseAndSetIfChanged(ref _player, value);
        }
        
        public string InstructionText
        {
            get => _instructionText;
            set => this.RaiseAndSetIfChanged(ref _instructionText, value);
        }
        
        public string ProgressText
        {
            get => _progressText;
            set => this.RaiseAndSetIfChanged(ref _progressText, value);
        }
        
        public List<Level.Platform> TutorialPlatforms => _tutorialPlatforms;
        
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
        
        public bool CanContinue => _hasReachedEnd;
        
        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }
        
        // ============= MÉTODOS PRIVADOS =============
        
        private async void InitializeTutorial()
        {
            try
            {
                Console.WriteLine(" Inicializando tutorial...");
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                
                Player = await playerRepo.GetByIdAsync(_playerId);
                if (Player == null)
                {
                    Console.WriteLine(" Jugador no encontrado");
                    return;
                }
                
                // Posición inicial del tutorial
                Player.Position.X = 100;
                Player.Position.Y = 500;
                Player.Velocity.X = 0;
                Player.Velocity.Y = 0;
                Player.IsJumping = false;
                
                UpdateInstructions();
                StartGameLoop();
                
                Console.WriteLine(" Tutorial iniciado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error iniciando tutorial: {ex.Message}");
            }
        }
        
        private void StartGameLoop()
        {
            _gameLoopCts = new CancellationTokenSource();
            _isRunning = true;
            
            Task.Run(async () =>
            {
                while (_isRunning && !_gameLoopCts.Token.IsCancellationRequested)
                {
                    GameLoop();
                    await Task.Delay(16);
                }
            }, _gameLoopCts.Token);
        }
        
        private void GameLoop()
        {
            if (Player == null) return;
            
            UpdatePlayerMovement();
            _physics.ApplyGravity(Player);
            _physics.UpdatePosition(Player);
            _collision.CheckPlatformCollisions(Player, _tutorialPlatforms);
            
            // Límites del mundo
            if (Player.Position.X < 0) Player.Position.X = 0;
            if (Player.Position.X > 1240) Player.Position.X = 1240;
            
            // Caída al vacío
            if (Player.Position.Y > 800)
            {
                Player.Position.X = 100;
                Player.Position.Y = 500;
                Player.Velocity.X = 0;
                Player.Velocity.Y = 0;
                Player.IsJumping = false;
            }
            
            CheckTutorialProgress();
            
            this.RaisePropertyChanged(nameof(Player));
        }
        
        private void UpdatePlayerMovement()
        {
            if (KeyLeft)
            {
                Player.Velocity.X = -PLAYER_SPEED;
                Player.IsFacingRight = false;
                _hasMovedLeft = true;
            }
            else if (KeyRight)
            {
                Player.Velocity.X = PLAYER_SPEED;
                Player.IsFacingRight = true;
                _hasMovedRight = true;
            }
            else
            {
                Player.Velocity.X = 0;
            }
            
            if (KeyUp && !Player.IsJumping)
            {
                Player.Velocity.Y = JUMP_FORCE;
                Player.IsJumping = true;
                _hasJumped = true;
            }
        }
        
        private void CheckTutorialProgress()
        {
            // Paso 1: Aprender a moverse
            if (_currentStep == 0 && _hasMovedLeft && _hasMovedRight)
            {
                _currentStep = 1;
                UpdateInstructions();
            }
            
            // Paso 2: Aprender a saltar
            if (_currentStep == 1 && _hasJumped)
            {
                _currentStep = 2;
                UpdateInstructions();
            }
            
            // Paso 3: Llegar a la meta
            if (_currentStep == 2 && Player.Position.X > 1000)
            {
                _currentStep = 3;
                _hasReachedEnd = true;
                UpdateInstructions();
                this.RaisePropertyChanged(nameof(CanContinue));
            }
        }
        
        private void UpdateInstructions()
        {
            switch (_currentStep)
            {
                case 0:
                    InstructionText = "BIENVENIDO AL TUTORIAL\n\nUsa ← y → (o A y D) para moverte";
                    ProgressText = "Paso 1/3: Aprender a moverse";
                    break;
                    
                case 1:
                    InstructionText = "¡Bien hecho!\n\nAhora usa ↑ (o W) para saltar";
                    ProgressText = "Paso 2/3: Aprender a saltar";
                    break;
                    
                case 2:
                    InstructionText = "¡Excelente!\n\nAhora llega hasta el final usando saltos y movimiento";
                    ProgressText = "Paso 3/3: Llegar a la meta";
                    break;
                    
                case 3:
                    InstructionText = "¡TUTORIAL COMPLETADO!\n\nYa dominas los controles básicos.\nPresiona CONTINUAR para elegir tu facción.";
                    ProgressText = "¡Tutorial completado!";
                    break;
            }
        }
        
        private async void OnContinue()
        {
            if (!CanContinue) return;
            
            Console.WriteLine("Tutorial completado - Navegando a selección de facción");
            StopGameLoop();
            
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                _navigate(new FactionSelectViewModel(_navigate, _playerId, _playerName));
            });
        }
        
        private void StopGameLoop()
        {
            _isRunning = false;
            _gameLoopCts?.Cancel();
        }
    }
}