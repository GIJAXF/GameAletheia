using ReactiveUI;
using System;
using System.Reactive;
using System.Collections.ObjectModel;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace GameAletheiaCross.ViewModels
{
    public class RankingViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        
        public RankingViewModel(Action<ViewModelBase> navigate)
        {
            _navigate = navigate;
            
            TopPlayers = new ObservableCollection<RankingEntry>();
            
            BackCommand = ReactiveCommand.Create(OnBack);
            
            Task.Run(LoadRanking);
        }
        
        public ObservableCollection<RankingEntry> TopPlayers { get; }
        
        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        
        private async Task LoadRanking()
        {
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                var topPlayers = await playerRepo.GetTopPlayersByScoreAsync(10);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    TopPlayers.Clear();
                    
                    int position = 1;
                    foreach (var player in topPlayers)
                    {
                        var medal = position switch
                        {
                            1 => "1.",
                            2 => "2.",
                            3 => "3.",
                            _ => $"{position:D2}."
                        };
                        
                        TopPlayers.Add(new RankingEntry
                        {
                            Position = position,
                            Medal = medal,
                            PlayerName = player.Name,
                            Score = player.TotalScore,
                            Level = player.CurrentLevel,
                            Faction = player.Faction ?? "Sin facción"
                        });
                        
                        position++;
                    }
                });
                
                Console.WriteLine($"  Ranking cargado: {topPlayers.Count} jugadores");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"  Error cargando ranking: {ex.Message}");
            }
        }
        
        private void OnBack()
        {
            _navigate(new MainMenuViewModel(_navigate));
        }
    }
    
    public class RankingEntry
    {
        public int Position { get; set; }
        public string Medal { get; set; }
        public string PlayerName { get; set; }
        public int Score { get; set; }
        public int Level { get; set; }
        public string Faction { get; set; }
    }
}