using ReactiveUI;
using System;
using System.Reactive;
using System.Collections.ObjectModel;
using GameAletheiaCross.Models;
using GameAletheiaCross.Services.Database;
using GameAletheiaCross.Services.Database.Repositories;
using System.Linq;
using MongoDB.Driver;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace GameAletheiaCross.ViewModels
{
    public class FactionSelectViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly string _playerId;
        private readonly string _playerName;
        private Faction _selectedFaction;
        
        public FactionSelectViewModel(Action<ViewModelBase> navigate, string playerId, string playerName)
        {
            _navigate = navigate;
            _playerId = playerId;
            _playerName = playerName;
            
            Factions = new ObservableCollection<Faction>();
            
            SelectFactionCommand = ReactiveCommand.Create<Faction>(OnSelectFaction);
            BackCommand = ReactiveCommand.Create(OnBack);
            
            // Cargar facciones
            Task.Run(LoadFactions);
        }
        
        public ObservableCollection<Faction> Factions { get; }
        
        public Faction SelectedFaction
        {
            get => _selectedFaction;
            set => this.RaiseAndSetIfChanged(ref _selectedFaction, value);
        }
        
        public ReactiveCommand<Faction, Unit> SelectFactionCommand { get; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        
        private async Task LoadFactions()
        {
            try
            {
                var dbService = new MongoDbService();
                var factionsCollection = dbService.GetCollection<Faction>("factions");
                var factions = await factionsCollection.Find(_ => true).ToListAsync();
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    Factions.Clear();
                    foreach (var faction in factions)
                    {
                        Factions.Add(faction);
                    }
                });
                
                Console.WriteLine($"✓ {factions.Count} facciones cargadas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error cargando facciones: {ex.Message}");
            }
        }
        
        private async void OnSelectFaction(Faction faction)
        {
            if (faction == null) return;
            
            SelectedFaction = faction;
            
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                await playerRepo.UpdateFactionAsync(_playerId, faction.Name);
                
                Console.WriteLine($"✓ Facción seleccionada: {faction.Name}");
                
                await Task.Delay(400);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _navigate(new GameViewModel(_navigate, _playerId, _playerName));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Error seleccionando facción: {ex.Message}");
            }
        }
        
        private void OnBack()
        {
            _navigate(new MainMenuViewModel(_navigate));
        }
    }
}