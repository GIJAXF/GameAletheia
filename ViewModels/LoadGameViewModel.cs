using ReactiveUI;
using System;
using System.Reactive;
using System.Collections.ObjectModel;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace GameAletheiaCross.ViewModels
{
    public class LoadGameViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private Player _selectedPlayer;
        
        public LoadGameViewModel(Action<ViewModelBase> navigate)
        {
            _navigate = navigate;
            
            Players = new ObservableCollection<Player>();
            
            SelectPlayerCommand = ReactiveCommand.Create<Player>(OnSelectPlayer);
            BackCommand = ReactiveCommand.Create(OnBack);
            
            // Cargar jugadores
            Task.Run(LoadPlayers);
        }
        
        public ObservableCollection<Player> Players { get; }
        
        public Player SelectedPlayer
        {
            get => _selectedPlayer;
            set => this.RaiseAndSetIfChanged(ref _selectedPlayer, value);
        }
        
        public ReactiveCommand<Player, Unit> SelectPlayerCommand { get; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        
        private async Task LoadPlayers()
        {
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                var players = await playerRepo.GetAllAsync();
                
                // Ordenar por fecha de última partida jugada
                var sortedPlayers = players.OrderByDescending(p => p.LastPlayed).ToList();
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Players.Clear();
                    foreach (var player in sortedPlayers)
                    {
                        Players.Add(player);
                    }
                });
                
                if (sortedPlayers.Count == 0)
                {
                    Console.WriteLine(" ️ No hay jugadores guardados");
                }
                else
                {
                    Console.WriteLine($"  {sortedPlayers.Count} jugadores cargados");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error cargando jugadores: {ex.Message}");
            }
        }
        
        private async void OnSelectPlayer(Player player)
        {
            if (player == null) return;
            
            SelectedPlayer = player;
            
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                
                // Actualizar última fecha jugada
                player.LastPlayed = DateTime.UtcNow;
                await playerRepo.UpdateAsync(player.Id, player);
                
                Console.WriteLine($"  Cargando jugador: {player.Name} (Nivel {player.CurrentLevel})");
                
                await Task.Delay(400);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _navigate(new GameViewModel(_navigate, player.Id, player.Name));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error cargando jugador: {ex.Message}");
            }
        }
        
        private void OnBack()
        {
            _navigate(new MainMenuViewModel(_navigate));
        }
    }
}