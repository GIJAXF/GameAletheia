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
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace GameAletheiaCross.ViewModels
{
    public class FactionSelectViewModel : ViewModelBase
    {
        private readonly Action<ViewModelBase> _navigate;
        private readonly string _playerId;
        private readonly string _playerName;
        private Faction _selectedFaction;
        private bool _showIntroduction = false;
        private Bitmap _backgroundImage;
        
        public FactionSelectViewModel(Action<ViewModelBase> navigate, string playerId, string playerName)
        {
            _navigate = navigate;
            _playerId = playerId;
            _playerName = playerName;
            
            Factions = new ObservableCollection<Faction>();
            
            SelectFactionCommand = ReactiveCommand.Create<Faction>(OnSelectFaction);
            ConfirmFactionCommand = ReactiveCommand.Create(OnConfirmFaction);
            CancelSelectionCommand = ReactiveCommand.Create(OnCancelSelection);
            BackCommand = ReactiveCommand.Create(OnBack);
            
            // Cargar fondo neutral por defecto
            LoadBackground("DefaultFondo.png");
            
            // Cargar facciones
            Task.Run(LoadFactions);
        }
        
        public ObservableCollection<Faction> Factions { get; }
        
        public Faction SelectedFaction
        {
            get => _selectedFaction;
            set => this.RaiseAndSetIfChanged(ref _selectedFaction, value);
        }
        
        public bool ShowIntroduction
        {
            get => _showIntroduction;
            set => this.RaiseAndSetIfChanged(ref _showIntroduction, value);
        }
        
        public Bitmap BackgroundImage
        {
            get => _backgroundImage;
            set => this.RaiseAndSetIfChanged(ref _backgroundImage, value);
        }
        
        public ReactiveCommand<Faction, Unit> SelectFactionCommand { get; }
        public ReactiveCommand<Unit, Unit> ConfirmFactionCommand { get; }
        public ReactiveCommand<Unit, Unit> CancelSelectionCommand { get; }
        public ReactiveCommand<Unit, Unit> BackCommand { get; }
        
        private void LoadBackground(string filename)
        {
            try
            {
                var uri = new Uri($"avares://GameAletheiaCross/Assets/Images/{filename}");
                var stream = AssetLoader.Open(uri);
                BackgroundImage = new Bitmap(stream);
                Console.WriteLine($"✅ Fondo cargado: {filename}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ No se pudo cargar {filename}: {ex.Message}");
                // Cargar fondo por defecto
                try
                {
                    var uri = new Uri("avares://GameAletheiaCross/Assets/Images/DefaultFondo.png");
                    var stream = AssetLoader.Open(uri);
                    BackgroundImage = new Bitmap(stream);
                }
                catch { }
            }
        }
        
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
                        // 🔍 DEBUG: Mostrar nombre exacto de cada facción
                        Console.WriteLine($"🏛️ Facción cargada: '{faction.Name}'");
                    }
                });
                
                Console.WriteLine($"✅ {factions.Count} facciones cargadas");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error cargando facciones: {ex.Message}");
            }
        }
        
        private void OnSelectFaction(Faction faction)
        {
            if (faction == null) return;
            
            SelectedFaction = faction;
            
            // Cargar fondo según facción (usando Contains para ser más flexible)
            string backgroundFile = "NeutralFondo.png"; // Default
            
            if (faction.Name.Contains("Gobierno") || faction.Name.Contains("Aletheia"))
            {
                backgroundFile = "GobiernoFondo.png";
            }
            else if (faction.Name.Contains("Redline") || faction.Name.Contains("Corporation"))
            {
                backgroundFile = "RedlineFondo.png";
            }
            else if (faction.Name.Contains("Resistencia") || faction.Name.Contains("Biblioteca"))
            {
                backgroundFile = "BibliotecaFondo.png";
            }
            
            Console.WriteLine($"🎨 Cambiando fondo a: {backgroundFile} para facción: {faction.Name}");
            
            // ⭐ IMPORTANTE: Usar Dispatcher para actualizar UI en el hilo correcto
            Dispatcher.UIThread.Post(() =>
            {
                LoadBackground(backgroundFile);
                ShowIntroduction = true;
            });
            
            Console.WriteLine($"✅ Facción seleccionada: {faction.Name}");
        }
        
        private async void OnConfirmFaction()
        {
            if (SelectedFaction == null) return;
            
            try
            {
                var dbService = new MongoDbService();
                var playerRepo = new PlayerRepository(dbService);
                await playerRepo.UpdateFactionAsync(_playerId, SelectedFaction.Name);
                
                Console.WriteLine($"✅ Facción confirmada: {SelectedFaction.Name}");
                
                await Task.Delay(400);
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    _navigate(new GameViewModel(_navigate, _playerId, _playerName));
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error confirmando facción: {ex.Message}");
            }
        }
        
        private void OnCancelSelection()
        {
            // ⭐ Volver al fondo neutral al cancelar
            Dispatcher.UIThread.Post(() =>
            {
                ShowIntroduction = false;
                SelectedFaction = null;
                LoadBackground("NeutralFondo.png");
            });
        }
        
        private void OnBack()
        {
            _navigate(new MainMenuViewModel(_navigate));
        }
    }
}