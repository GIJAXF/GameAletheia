using ReactiveUI;
using System;
using System.Reactive;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using Avalonia.Threading;
using System.Threading.Tasks;

namespace GameAletheiaCross.ViewModels
{
    public class CharacterSelectViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private string _playerName;
        private bool _isMale = true;
        
        public CharacterSelectViewModel(Action<ViewModelBase> navigate)
        {
            _navigate = navigate;
            ContinueCommand = ReactiveCommand.Create(OnContinue);
            BackCommand = ReactiveCommand.Create(OnBack);
        }
        
        public string PlayerName
        {
            get => _playerName;
            set => this.RaiseAndSetIfChanged(ref _playerName, value);
        }
        
        public bool IsMale
        {
            get => _isMale;
            set => this.RaiseAndSetIfChanged(ref _isMale, value);
        }
        
        public ReactiveCommand<Unit, Unit> ContinueCommand { get; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; }

        private async void OnContinue()
        {
            if (string.IsNullOrWhiteSpace(PlayerName))
            {
                Console.WriteLine("⚠️ Debes ingresar un nombre");
                return;
            }

            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);

                var player = new Player
                {
                    Name = PlayerName.Trim(),
                    Gender = IsMale ? "Hombre" : "Mujer",
                    Avatar = "default",
                    CurrentLevel = 1
                };

                await playerRepo.CreateAsync(player);
                Console.WriteLine($"✓ Jugador creado: {player.Name} (ID: {player.Id})");

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _navigate(new FactionSelectViewModel(_navigate, player.Id, player.Name));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error creando jugador: {ex.Message}");
            }
        }
        
        private void OnBack()
        {
            _navigate(new MainMenuViewModel(_navigate));
        }
    }
}