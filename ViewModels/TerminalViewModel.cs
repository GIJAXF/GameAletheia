using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;
using Avalonia.Threading;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services;
using GameAletheiaCross.Services.Database;

namespace GameAletheiaCross.ViewModels
{
    public class TerminalViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly string _playerId;
        private readonly string _levelId;
        private readonly GameViewModel _gameViewModel;
        
        private Puzzle? _currentPuzzle;
        private string _code = "";
        private string _output = "";
        private int _currentHintIndex = 0;
        private bool _isCompiling = false;
        
        private readonly JavaCompilerService _compiler;
        private readonly PuzzleService _puzzleService;
        
        public TerminalViewModel(Action<ViewModelBase> navigate, string playerId, string levelId, GameViewModel gameViewModel)
        {
            _navigate = navigate;
            _playerId = playerId;
            _levelId = levelId;
            _gameViewModel = gameViewModel;
            
            var dbService = new MongoDbService();
            _compiler = new JavaCompilerService();
            _puzzleService = new PuzzleService(dbService);
            
            CompileCommand = ReactiveCommand.Create(OnCompile, this.WhenAnyValue(x => x.IsCompiling, compiling => !compiling));
            ShowHintCommand = ReactiveCommand.Create(OnShowHint);
            CloseCommand = ReactiveCommand.Create(OnClose);
            ResetCodeCommand = ReactiveCommand.Create(OnResetCode);
            
            LoadPuzzle();
        }
        
        // ============= PROPIEDADES =============
        
        public Puzzle? CurrentPuzzle
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

        public bool IsCompiling
        {
            get => _isCompiling;
            set => this.RaiseAndSetIfChanged(ref _isCompiling, value);
        }

        public string PuzzleTitle => CurrentPuzzle != null 
            ? $"{CurrentPuzzle.Name} [{CurrentPuzzle.Type}] - {CurrentPuzzle.Points} pts" 
            : "Sin puzzle";

        public int TotalHints => CurrentPuzzle?.Hints?.Count ?? 0;
        public int HintsUsed => _currentHintIndex;
        
        // ============= COMANDOS =============
        
        public ReactiveCommand<Unit, Unit> CompileCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowHintCommand { get; }
        public ReactiveCommand<Unit, Unit> CloseCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCodeCommand { get; }
        
        // ============= MÉTODOS PRIVADOS =============
        
        private async void LoadPuzzle()
        {
            try
            {
                CurrentPuzzle = await _puzzleService.GetPuzzleForLevelAsync(_levelId);
                
                if (CurrentPuzzle != null)
                {
                    Code = CurrentPuzzle.StarterCode ?? "";
                    Output = BuildWelcomeMessage();
                    Console.WriteLine($"  Puzzle cargado: {CurrentPuzzle.Name}");
                    this.RaisePropertyChanged(nameof(PuzzleTitle));
                }
                else
                {
                    Output = " ️ No hay puzzles disponibles para este nivel.\n\nPuedes continuar explorando.";
                    Console.WriteLine(" ️ No hay puzzles en este nivel");
                }
            }
            catch (Exception ex)
            {
                Output = $"  Error cargando puzzle: {ex.Message}";
                Console.WriteLine($"  Error: {ex.Message}");
            }
        }

        private string BuildWelcomeMessage()
        {
            if (CurrentPuzzle == null) return "";

            return $@"╔═══════════════════════════════════════════════════════╗
║    {CurrentPuzzle.Name.ToUpper()}
║  Dificultad: {" ".PadRight(CurrentPuzzle.Difficulty, ' ')}{" ".PadRight(5 - CurrentPuzzle.Difficulty, ' ')}
║  Recompensa: {CurrentPuzzle.Points} puntos
╚═══════════════════════════════════════════════════════╝

 DESCRIPCIÓN:
{CurrentPuzzle.Description}

  Tienes {CurrentPuzzle.Hints?.Count ?? 0} pistas disponibles
▶ Escribe tu código y presiona COMPILAR cuando estés listo
";
        }
        
        private async void OnCompile()
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                Output = " ️ ERROR: El código está vacío\n\nEscribe tu solución antes de compilar.";
                return;
            }
            
            if (CurrentPuzzle == null)
            {
                Output = "  ERROR: No hay puzzle cargado";
                return;
            }

            IsCompiling = true;
            Output = "  Compilando y ejecutando tu código...\n\nPor favor espera...";
            
            try
            {
                // Compilar y ejecutar el código
                var compilationResult = await _compiler.CompileAndRunAsync(Code, CurrentPuzzle.ExpectedOutput);
                
                if (compilationResult.Success)
                {
                    // Validar con el servicio de puzzles
                    var validationResult = await _puzzleService.ValidateSolutionAsync(
                        _playerId, 
                        CurrentPuzzle.Id, 
                        Code, 
                        compilationResult.Output);
                    
                    if (validationResult.IsValid)
                    {
                        await HandleSuccessfulSolution(validationResult);
                    }
                    else
                    {
                        HandleIncorrectSolution(compilationResult.Output);
                    }
                }
                else
                {
                    HandleCompilationError(compilationResult.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                Output = $" ERROR INESPERADO\n\n{ex.Message}\n\nVerifica que JDK esté instalado correctamente.";
                Console.WriteLine($"  Error de compilación: {ex.Message}");
            }
            finally
            {
                IsCompiling = false;
            }
        }

        private async Task HandleSuccessfulSolution(PuzzleValidationResult result)
        {
            Output = $@"
╔═══════════════════════════════════════════════════════╗
║   ¡PUZZLE RESUELTO CORRECTAMENTE!
╚═══════════════════════════════════════════════════════╝

  ¡Excelente trabajo!
  Has ganado {result.PointsEarned} puntos

  Estadísticas:
   • Pistas usadas: {_currentHintIndex}/{TotalHints}
   • Tiempo: --:--

{CurrentPuzzle!.Name} completado exitosamente.

La terminal se cerrará en 3 segundos...
";
            
            Console.WriteLine($"  Puzzle resuelto: {CurrentPuzzle.Name} (+{result.PointsEarned} pts)");
            
            // Notificar al GameViewModel que el puzzle fue resuelto
            await _gameViewModel.OnPuzzleCompletedAsync(CurrentPuzzle.Id);
            
            // Esperar y cerrar
            await Task.Delay(3000);
            OnClose();
        }

        private void HandleIncorrectSolution(string actualOutput)
        {
            Output = $@" SOLUCIÓN INCORRECTA

Tu código compiló correctamente pero la salida no es la esperada.

  Salida esperada:
{CurrentPuzzle!.ExpectedOutput}

  Tu salida:
{actualOutput}

  Revisa tu lógica e intenta nuevamente.
  Puedes usar el botón PISTA si necesitas ayuda.
";
            Console.WriteLine("  Solución incorrecta");
        }

        private void HandleCompilationError(string errorMessage)
        {
            Output = $@" ERROR DE COMPILACIÓN

Tu código tiene errores de sintaxis:

{errorMessage}

  Consejos:
   • Verifica punto y comas (;)
   • Revisa llaves {{ }} correctamente cerradas
   • Asegúrate de que las variables estén declaradas
   • Verifica nombres de métodos (case-sensitive)

Usa el botón PISTA si necesitas ayuda.
";
            Console.WriteLine("  Error de compilación");
        }
        
        private void OnShowHint()
        {
            if (CurrentPuzzle == null)
            {
                Output = " ️ No hay puzzle cargado";
                return;
            }

            var hint = _puzzleService.GetNextHint(CurrentPuzzle, _currentHintIndex);
            
            if (_currentHintIndex < TotalHints)
            {
                _currentHintIndex++;
                Output = $@"
  PISTA {_currentHintIndex}/{TotalHints}

{hint}

{(_currentHintIndex < TotalHints ? "Presiona PISTA nuevamente para ver más pistas." : "No hay más pistas disponibles.")}
";
                this.RaisePropertyChanged(nameof(HintsUsed));
            }
            else
            {
                Output = $@"
 ️ NO HAY MÁS PISTAS

Ya has usado todas las {TotalHints} pistas disponibles.
¡Ahora depende de ti resolverlo!

Recuerda:
{BuildWelcomeMessage()}
";
            }
        }

        private void OnResetCode()
        {
            if (CurrentPuzzle != null)
            {
                Code = CurrentPuzzle.StarterCode ?? "";
                Output = "  Código reiniciado al estado inicial.\n\n" + BuildWelcomeMessage();
                Console.WriteLine("  Código reiniciado");
            }
        }
        
        private void OnClose()
        {
            Console.WriteLine("  Terminal cerrada");
            _gameViewModel.ReturnFromSubView();
            _navigate(_gameViewModel);
        }
    }
}