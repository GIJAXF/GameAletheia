using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GameAletheiaCross.Models;
using GameAletheiaCross.ViewModels;
using System;
using System.Collections.Generic;

namespace GameAletheiaCross.Views
{
    public partial class GameView : UserControl
    {
        private Canvas? _gameCanvas;
        private GameViewModel? _viewModel;
        
        private Image? _playerImage;
        private Image? _floorImage;
        private readonly List<Image> _platformImages = new();
        private readonly List<Image> _npcImages = new();
        private readonly List<TextBlock> _npcNameLabels = new();
        private Image? _portalImage;
        private Avalonia.Controls.Shapes.Ellipse? _portalGlow;
        
        private Bitmap? _playerMaleBitmap;
        private Bitmap? _playerFemaleBitmap;
        private readonly Dictionary<string, Bitmap?> _npcBitmapCache = new();
        private Bitmap? _portalBitmap;
        private readonly Dictionary<int, Bitmap?> _platformSpriteCache = new();
        private readonly Dictionary<int, Bitmap?> _floorSpriteCache = new();
        
        private bool _spritesLoaded = false;
        private bool _levelRendered = false;
        private int _lastRenderedLevel = -1;

        private ScaleTransform? _playerFlipTransform;
        private float _lastPlayerX = -1;
        private float _lastPlayerY = -1;
        private bool _lastFacingRight = true;

        // ✅ TIMER PARA ACTUALIZACIÓN DIRECTA (sin PropertyChanged)
        private DispatcherTimer? _updateTimer;

        public GameView()
        {
            InitializeComponent();
            Console.WriteLine("🎮 GameView inicializado");
            
            LoadAllSprites();
            
            Focusable = true;
        }

        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnAttachedToVisualTree(e);
            
            Focus();
            
            _gameCanvas = this.FindControl<Canvas>("GameCanvas");
            
            if (_gameCanvas == null)
            {
                Console.WriteLine("❌ ERROR: No se pudo encontrar GameCanvas");
                return;
            }
            
            _viewModel = (GameViewModel?)DataContext;
            
            if (_viewModel == null)
            {
                Console.WriteLine("❌ ERROR: ViewModel es null");
                return;
            }
            
            // ✅ SOLO escuchar cambios de nivel (no de Player)
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            if (_viewModel.CurrentLevel != null)
            {
                Dispatcher.UIThread.Post(RenderLevel);
            }
            
            // ✅ INICIAR TIMER DE ACTUALIZACIÓN (60 FPS)
            StartUpdateTimer();
            
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        // ✅ TIMER QUE ACTUALIZA LA POSICIÓN DIRECTAMENTE
        private void StartUpdateTimer()
        {
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(16) // ~60 FPS
            };
            
            _updateTimer.Tick += (s, e) => UpdatePlayerPositionDirect();
            _updateTimer.Start();
            
            Console.WriteLine("⏱️ Timer de actualización iniciado");
        }

        private void LoadAllSprites()
        {
            Console.WriteLine("📦 Pre-cargando todos los sprites...");
            
            try
            {
                _playerMaleBitmap = LoadBitmap("playerH.png");
                _playerFemaleBitmap = LoadBitmap("playerM.png");
                
                _npcBitmapCache["npc1"] = LoadBitmap("npc1.png");
                _npcBitmapCache["npc2"] = LoadBitmap("npc2.png");
                _npcBitmapCache["npc3"] = LoadBitmap("npc3.png");
                _npcBitmapCache["npc4"] = LoadBitmap("npc4.png");
                
                _portalBitmap = LoadBitmap("portal.png");
                
                for (int i = 1; i <= 7; i++)
                {
                    _platformSpriteCache[i] = LoadBitmap($"platform{i}.png");
                }
                
                _floorSpriteCache[1] = LoadBitmap("pastopiso.png");
                _floorSpriteCache[2] = LoadBitmap("piedraspiso.png");
                _floorSpriteCache[3] = LoadBitmap("gobierno.png");
                _floorSpriteCache[4] = LoadBitmap("nievepiso.png");
                _floorSpriteCache[5] = LoadBitmap("piedratutorial.png");
                _floorSpriteCache[6] = LoadBitmap("redlinepiso.png");
                _floorSpriteCache[7] = LoadBitmap("piedraspiso.png");
                
                _spritesLoaded = true;
                
                Console.WriteLine($"✅ Todos los sprites pre-cargados en memoria");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error pre-cargando sprites: {ex.Message}");
                _spritesLoaded = false;
            }
        }
        
        private Bitmap? LoadBitmap(string filename)
        {
            try
            {
                var uri = new Uri($"avares://GameAletheiaCross/Assets/Images/{filename}");
                using var stream = AssetLoader.Open(uri);
                return new Bitmap(stream);
            }
            catch
            {
                return null;
            }
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ✅ SOLO re-renderizar cuando cambia el nivel
            if (e.PropertyName == nameof(GameViewModel.CurrentLevel))
            {
                Dispatcher.UIThread.Post(RenderLevel);
            }
            // ✅ NO escuchar cambios de Player (se actualiza por timer)
        }

        private void RenderLevel()
        {
            if (_gameCanvas == null || _viewModel?.CurrentLevel == null)
                return;

            var level = _viewModel.CurrentLevel;
            
            if (_lastRenderedLevel == level.OrderNumber && _levelRendered)
            {
                Console.WriteLine($"⚡ Nivel {level.OrderNumber} ya renderizado, saltando...");
                return;
            }
            
            Console.WriteLine($"🎨 Renderizando nivel {level.OrderNumber}...");
            
            _gameCanvas.Children.Clear();
            _platformImages.Clear();
            _npcImages.Clear();
            _npcNameLabels.Clear();
            
            RenderFloor(level.OrderNumber, level.Floor);
            RenderPlatforms(level.OrderNumber, level.Platforms);
            RenderNPCs(level.NPCs);
            RenderPortal();
            RenderPlayer();
            
            _levelRendered = true;
            _lastRenderedLevel = level.OrderNumber;
            Console.WriteLine($"✅ Nivel {level.OrderNumber} renderizado ({_gameCanvas.Children.Count} elementos)");
        }

        private void RenderFloor(int levelNumber, Level.Platform? floor)
        {
            if (floor == null) return;
            
            Bitmap? floorSprite = _floorSpriteCache.GetValueOrDefault(levelNumber);
            
            if (floorSprite != null)
            {
                _floorImage = new Image
                {
                    Source = floorSprite,
                    Width = floor.Width,
                    Height = floor.Height,
                    Stretch = Stretch.Fill
                };
                
                Canvas.SetLeft(_floorImage, floor.X);
                Canvas.SetTop(_floorImage, floor.Y);
                _gameCanvas!.Children.Add(_floorImage);
            }
            else
            {
                var rect = new Avalonia.Controls.Shapes.Rectangle
                {
                    Fill = new SolidColorBrush(GetLevelColor(levelNumber)),
                    Width = floor.Width,
                    Height = floor.Height
                };
                
                Canvas.SetLeft(rect, floor.X);
                Canvas.SetTop(rect, floor.Y);
                _gameCanvas!.Children.Add(rect);
            }
        }

        private void RenderPlatforms(int levelNumber, List<Level.Platform> platforms)
        {
            if (platforms == null || platforms.Count == 0) return;
            
            Bitmap? platformSprite = _platformSpriteCache.GetValueOrDefault(levelNumber);
            
            foreach (var platform in platforms)
            {
                if (platformSprite != null)
                {
                    var img = new Image
                    {
                        Source = platformSprite,
                        Width = platform.Width,
                        Height = platform.Height,
                        Stretch = Stretch.Fill
                    };
                    
                    Canvas.SetLeft(img, platform.X);
                    Canvas.SetTop(img, platform.Y);
                    _gameCanvas!.Children.Add(img);
                    _platformImages.Add(img);
                }
                else
                {
                    var rect = new Avalonia.Controls.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(GetLevelColor(levelNumber)),
                        Width = platform.Width,
                        Height = platform.Height
                    };
                    
                    Canvas.SetLeft(rect, platform.X);
                    Canvas.SetTop(rect, platform.Y);
                    _gameCanvas!.Children.Add(rect);
                }
            }
        }

        private void RenderNPCs(List<Models.NPC> npcs)
        {
            if (npcs == null || npcs.Count == 0) return;
            
            foreach (var npc in npcs)
            {
                Bitmap? npcSprite = GetNPCSpriteByName(npc.Name);
                
                if (npcSprite != null)
                {
                    var img = new Image
                    {
                        Source = npcSprite,
                        Width = 32,
                        Height = 32
                    };
                    
                    Canvas.SetLeft(img, npc.PositionX - 16);
                    Canvas.SetTop(img, npc.PositionY - 32);
                    _gameCanvas!.Children.Add(img);
                    _npcImages.Add(img);
                }
                else
                {
                    var ellipse = new Avalonia.Controls.Shapes.Ellipse
                    {
                        Fill = new SolidColorBrush(Color.Parse("#00d9ff")),
                        Width = 40,
                        Height = 40
                    };
                    
                    Canvas.SetLeft(ellipse, npc.PositionX - 20);
                    Canvas.SetTop(ellipse, npc.PositionY - 20);
                    _gameCanvas!.Children.Add(ellipse);
                }
                
                var nameText = new TextBlock
                {
                    Text = npc.Name.Split('—')[0].Trim(),
                    FontSize = 10,
                    Foreground = new SolidColorBrush(Colors.White),
                    Background = new SolidColorBrush(Color.FromArgb(180, 0, 0, 0)),
                    Padding = new Thickness(4, 2),
                    TextAlignment = TextAlignment.Center
                };
                
                Canvas.SetLeft(nameText, npc.PositionX - 40);
                Canvas.SetTop(nameText, npc.PositionY - 50);
                _gameCanvas!.Children.Add(nameText);
                _npcNameLabels.Add(nameText);
            }
        }

        private void RenderPortal()
        {
            _portalGlow = new Avalonia.Controls.Shapes.Ellipse
            {
                Fill = new SolidColorBrush(Color.Parse("#00FF88")),
                Width = 60,
                Height = 120,
                Opacity = 0.3
            };
            
            Canvas.SetLeft(_portalGlow, 670);
            Canvas.SetTop(_portalGlow, 310);
            _gameCanvas!.Children.Add(_portalGlow);
            
            if (_portalBitmap != null)
            {
                _portalImage = new Image
                {
                    Source = _portalBitmap,
                    Width = 40,
                    Height = 100,
                    Opacity = 0.8,
                    Stretch = Stretch.Fill
                };
                
                Canvas.SetLeft(_portalImage, 680);
                Canvas.SetTop(_portalImage, 320);
                _gameCanvas!.Children.Add(_portalImage);
            }
        }

        private void RenderPlayer()
        {
            if (_viewModel?.Player == null) return;
            
            Bitmap? playerBitmap = _viewModel.Player.Gender == "Hombre" 
                ? _playerMaleBitmap 
                : _playerFemaleBitmap;
            
            if (playerBitmap != null)
            {
                _playerImage = new Image
                {
                    Source = playerBitmap,
                    Width = 32,
                    Height = 32
                };
                
                Canvas.SetLeft(_playerImage, _viewModel.Player.Position.X - 16);
                Canvas.SetTop(_playerImage, _viewModel.Player.Position.Y - 32);
                
                _gameCanvas!.Children.Add(_playerImage);
            }
        }

        // ✅ ACTUALIZACIÓN DIRECTA POR TIMER (sin PropertyChanged)
        private void UpdatePlayerPositionDirect()
        {
            if (_gameCanvas == null || _viewModel?.Player == null || _playerImage == null)
                return;

            var player = _viewModel.Player;
            
            float newX = player.Position.X - 16;
            float newY = player.Position.Y - 32;
            
            // ✅ Actualizar solo si cambió significativamente
            if (Math.Abs(newX - _lastPlayerX) > 0.5f || Math.Abs(newY - _lastPlayerY) > 0.5f)
            {
                Canvas.SetLeft(_playerImage, newX);
                Canvas.SetTop(_playerImage, newY);
                _lastPlayerX = newX;
                _lastPlayerY = newY;
            }
            
            // ✅ Actualizar dirección
            if (player.IsFacingRight != _lastFacingRight)
            {
                if (player.IsFacingRight)
                {
                    _playerImage.RenderTransform = null;
                }
                else
                {
                    if (_playerFlipTransform == null)
                    {
                        _playerFlipTransform = new ScaleTransform(-1, 1);
                        _playerImage.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                    }
                    _playerImage.RenderTransform = _playerFlipTransform;
                }
                _lastFacingRight = player.IsFacingRight;
            }
        }

        private Bitmap? GetNPCSpriteByName(string npcName)
        {
            string cleanName = npcName.Split('—')[0].Trim().ToLower();
            
            if (cleanName.Contains("custodio") || cleanName.Contains("reportero") || 
                cleanName.Contains("bibliotecario") || cleanName.Contains("guardián"))
                return _npcBitmapCache.GetValueOrDefault("npc1");
            else if (cleanName.Contains("decano") || cleanName.Contains("villanueva"))
                return _npcBitmapCache.GetValueOrDefault("npc2");
            else if (cleanName.Contains("noa") || cleanName.Contains("espectra"))
                return _npcBitmapCache.GetValueOrDefault("npc3");
            else if (cleanName.Contains("archivero") || cleanName.Contains("julián"))
                return _npcBitmapCache.GetValueOrDefault("npc4");
            else if (cleanName.Contains("ia") || cleanName.Contains("centinela") || 
                     cleanName.Contains("analista"))
                return _npcBitmapCache.GetValueOrDefault("npc2");
            
            return _npcBitmapCache.GetValueOrDefault("npc1");
        }

        private Color GetLevelColor(int levelNumber)
        {
            return levelNumber switch
            {
                1 => Color.Parse("#9D4EDD"),
                2 => Color.Parse("#06D6A0"),
                3 => Color.Parse("#FF9E00"),
                4 => Color.Parse("#00D9FF"),
                5 => Color.Parse("#0080FF"),
                6 => Color.Parse("#FFD60A"),
                7 => Color.Parse("#E63946"),
                _ => Color.Parse("#FFFFFF")
            };
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            
            // ✅ DETENER TIMER
            _updateTimer?.Stop();
            _updateTimer = null;
            
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= ViewModel_PropertyChanged;
            }
            
            KeyDown -= OnKeyDown;
            KeyUp -= OnKeyUp;
            
            _gameCanvas?.Children.Clear();
            
            Console.WriteLine("🧹 GameView limpiado");
        }

        private void OnKeyDown(object? sender, KeyEventArgs e)
        {
            if (DataContext is not GameViewModel vm) return;

            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    vm.KeyLeft = true;
                    break;

                case Key.D:
                case Key.Right:
                    vm.KeyRight = true;
                    break;

                case Key.W:
                case Key.Up:
                    vm.KeyUp = true;
                    break;

                case Key.Space:
                    vm.KeySpace = true;
                    break;

                case Key.Escape:
                    vm.OnPauseRequested();
                    break;

                case Key.T:
                    vm.OnToggleTerminal();
                    break;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (DataContext is not GameViewModel vm) return;

            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    vm.KeyLeft = false;
                    break;

                case Key.D:
                case Key.Right:
                    vm.KeyRight = false;
                    break;

                case Key.W:
                case Key.Up:
                    vm.KeyUp = false;
                    break;

                case Key.Space:
                    vm.KeySpace = false;
                    break;
            }
        }
    }
}