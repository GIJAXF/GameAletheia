using ReactiveUI;
using System.Reactive.Concurrency;
using Avalonia.Threading;
using System;
using System.Reactive;

namespace GameAletheiaCross.ViewModels
{
    public class MainMenuViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;

        public MainMenuViewModel(Action<ViewModelBase> navigate)
        {
            _navigate = navigate;

            // 🔧 Fijamos el scheduler al hilo principal de Avalonia:
            var mainThread = RxApp.MainThreadScheduler;

            NewGameCommand = ReactiveCommand.Create(OnNewGame, outputScheduler: mainThread);
            LoadGameCommand = ReactiveCommand.Create(OnLoadGame, outputScheduler: mainThread);
            ShowRankingCommand = ReactiveCommand.Create(OnShowRanking, outputScheduler: mainThread);
            ExitCommand = ReactiveCommand.Create(OnExit, outputScheduler: mainThread);
        }

        public ReactiveCommand<Unit, Unit> NewGameCommand { get; }
        public ReactiveCommand<Unit, Unit> LoadGameCommand { get; }
        public ReactiveCommand<Unit, Unit> ShowRankingCommand { get; }
        public ReactiveCommand<Unit, Unit> ExitCommand { get; }

        private void OnNewGame()
        {
            try
            {
                // Aseguramos navegación en hilo de UI:
                Dispatcher.UIThread.Post(() =>
                {
                    Console.WriteLine("Navegando a selección de personaje...");
                    _navigate(new CharacterSelectViewModel(_navigate));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en OnNewGame: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void OnLoadGame()
        {
            try
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Console.WriteLine("Navegando a cargar partida...");
                    _navigate(new LoadGameViewModel(_navigate));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en OnLoadGame: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void OnShowRanking()
        {
            try
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Console.WriteLine("Navegando a ranking...");
                    _navigate(new RankingViewModel(_navigate));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error en OnShowRanking: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void OnExit() => Environment.Exit(0);
    }
}