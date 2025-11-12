using ReactiveUI;
using System;
using System.Reactive;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;

namespace GameAletheiaCross.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly string _playerId;
        private readonly string _levelId;
        private readonly GameViewModel _gameViewModel;
        
        private Puzzle _currentPuzzle;
        private string _code = "";
        private string _output = "";
        private int _currentHintIndex = 0;
        private readonly JavaCompilerService _compiler;
        private readonly LevelManager _levelManager;
        
        public TerminalViewModel(Action<ViewModelBase> navigate, string playerId, string levelId, GameViewModel gameViewModel)
        {
            _navigate = navigate;
            _playerId = playerId;
            _levelId = levelId;
            _gameViewModel = gameViewModel;
            
            var dbService = new MongoDbService();
            _compiler = new JavaCompilerService();
            _levelManager = new LevelManager(dbService);
            
            CompileCommand = ReactiveCommand.Create(OnCompile);
            ShowHintCommand = ReactiveCommand.Create(OnShowHint);
            CloseCommand = ReactiveCommand.Create(OnClose);
            
            LoadPuzzle();
        }
        
        public Puzzle CurrentPuzzle
        {
            get => _currentPuzzle;
            set => this.RaiseAndSetIfChanged(ref _currentPuzzle, value);
        }
        
        public string Code
        {
            get => _code;
            set => this.RaiseAndSetIfChanged(ref _code, value);
        }
        
        public string Output
        {
            get => _output;
            set => this.RaiseAndSetIfChanged(ref _output, value);
        }
        
        public ReactiveCommand<Unit, Unit> CompileCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowHintCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        
        private async void LoadPuzzle()
        {
            try
            {
                var dbService = new MongoDbService();
                var puzzleRepo = new PuzzleRepository(dbService);
                
                CurrentPuzzle = await puzzleRepo.GetByLevelIdAsync(_levelId);
                
                if (CurrentPuzzle != null)
                {
                    Code = CurrentPuzzle.StarterCode ?? "";
                    Output = $"🎯 {CurrentPuzzle.Name}\n\n{CurrentPuzzle.Description}\n\n▶ Escribe tu código y presiona COMPILAR";
                    Console.WriteLine($"✓ Puzzle cargado: {CurrentPuzzle.Name}");
                }
                else
                {
                    Output = "⚠️ No hay puzzle disponible para este nivel";
                    Console.WriteLine("✗ Puzzle no encontrado");
                }
            }
            catch (Exception ex)
            {
                Output = $"✗ Error cargando puzzle: {ex.Message}";
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
        
        private async void OnCompile()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                Output = "⚠️ Escribe código antes de compilar";
                return;
            }
            
            if (CurrentPuzzle == null)
            {
                Output = "✗ No hay puzzle cargado";
                return;
            }
            
            Output = "⏳ Compilando y ejecutando...";
            
            try
            {
                var result = await _compiler.CompileAndRunAsync(Code, CurrentPuzzle.ExpectedOutput);
                
                if (result.Success)
                {
                    Output = $"✅ ¡CORRECTO!\n\n" +
                            $"Salida esperada: {CurrentPuzzle.ExpectedOutput}\n" +
                            $"Tu salida: {result.Output}\n\n" +
                            $"🎉 ¡Puzzle resuelto! +{CurrentPuzzle.Points} puntos\n\n" +
                            $"El nivel continuará en 3 segundos...";
                    
                    Console.WriteLine($"✓ Puzzle resuelto: {CurrentPuzzle.Name}");
                    
                    // Registrar que completó el puzzle
                    await _levelManager.CompletePuzzleAsync(_playerId, CurrentPuzzle.Id);
                    
                    // Esperar antes de cerrar
                    await System.Threading.Tasks.Task.Delay(3000);
                    OnClose();
                }
                else
                {
                    Output = $"❌ ERROR DE COMPILACIÓN\n\n{result.ErrorMessage}\n\n" +
                            $"Revisa tu código e intenta de nuevo.";
                    Console.WriteLine($"✗ Compilación fallida");
                }
            }
            catch (Exception ex)
            {
                Output = $"❌ Error: {ex.Message}";
                Console.WriteLine($"✗ Error: {ex.Message}");
            }
        }
        
        private void OnShowHint()
        {
            if (CurrentPuzzle == null || CurrentPuzzle.Hints == null || CurrentPuzzle.Hints.Count == 0)
            {
                Output = "⚠️ No hay pistas disponibles";
                return;
            }
            
            if (_currentHintIndex < CurrentPuzzle.Hints.Count)
            {
                Output = $"💡 PISTA {_currentHintIndex + 1}/{CurrentPuzzle.Hints.Count}:\n\n" +
                        $"{CurrentPuzzle.Hints[_currentHintIndex]}\n\n" +
                        $"Presiona 'Mostrar Pista' nuevamente para ver más pistas.";
                _currentHintIndex++;
            }
            else
            {
                Output = "⚠️ No hay más pistas disponibles\n\n" +
                        "¡Ya has visto todas las pistas! Intenta resolver el puzzle.";
            }
        }
        
        private void OnClose()
        {
            _gameViewModel.ResumeGameLoop();
            _navigate(_gameViewModel);
        }
    }
}