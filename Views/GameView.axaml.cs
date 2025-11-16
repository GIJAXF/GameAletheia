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
        
        // ✅ REFERENCIAS A ELEMENTOS VISUALES (sin recrear en cada frame)
        private Image? _playerImage;
        private Image? _floorImage;
        private readonly List<Image> _platformImages = new();
        private readonly List<Image> _npcImages = new();
        private readonly List<TextBlock> _npcNameLabels = new();
        private Image? _portalImage;
        private Avalonia.Controls.Shapes.Ellipse? _portalGlow;
        
        // Cache de imágenes
        private Bitmap? _playerBitmap;
        private readonly List<Bitmap?> _npcBitmaps = new();
        private Bitmap? _portalBitmap;
        private readonly Dictionary<int, Bitmap?> _platformSpriteCache = new();
        private readonly Dictionary<int, Bitmap?> _floorSpriteCache = new();
        
        private bool _spritesLoaded = false;
        private bool _levelRendered = false;

        public GameView()
        {
            InitializeComponent();
            Console.WriteLine("🎮 GameView inicializado");
            
            LoadImages();
            
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
            
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            if (_viewModel.CurrentLevel != null)
            {
                Dispatcher.UIThread.Post(RenderLevel);
            }
            
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        private void LoadImages()
        {
            Console.WriteLine("📦 Cargando sprites...");
            
            try
            {
                // Cargar jugador (se actualizará dinámicamente según género)
                _playerBitmap = LoadBitmap("playerH.png");
                
                // Cargar NPCs
                _npcBitmaps.Add(LoadBitmap("npc1.png"));
                _npcBitmaps.Add(LoadBitmap("npc2.png"));
                _npcBitmaps.Add(LoadBitmap("npc3.png"));
                _npcBitmaps.Add(LoadBitmap("npc4.png"));
                
                // Cargar portal
                _portalBitmap = LoadBitmap("portal.png");
                
                _spritesLoaded = _playerBitmap != null;
                
                Console.WriteLine($"✅ Sprites cargados: Jugador={_playerBitmap != null}, NPCs={_npcBitmaps.Count}, Portal={_portalBitmap != null}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️ Error cargando sprites: {ex.Message}");
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
            if (e.PropertyName == nameof(GameViewModel.CurrentLevel))
            {
                _levelRendered = false; // Marcar que necesita re-renderizar
                Dispatcher.UIThread.Post(RenderLevel);
            }
            else if (e.PropertyName == nameof(GameViewModel.Player))
            {
                Dispatcher.UIThread.Post(UpdatePlayerPosition);
            }
        }

        // ✅ RENDERIZAR NIVEL (se llama solo 1 vez por nivel)
        private void RenderLevel()
        {
            if (_gameCanvas == null || _viewModel?.CurrentLevel == null)
                return;

            var level = _viewModel.CurrentLevel;
            
            Console.WriteLine($"🎨 Renderizando nivel {level.OrderNumber}...");
            
            // ⚠️ LIMPIAR TODO
            _gameCanvas.Children.Clear();
            _platformImages.Clear();
            _npcImages.Clear();
            _npcNameLabels.Clear();
            
            // Actualizar sprite del jugador según género
            if (_viewModel.Player != null)
            {
                string playerSprite = _viewModel.Player.Gender == "Hombre" ? "playerH.png" : "playerM.png";
                _playerBitmap = LoadBitmap(playerSprite);
            }

            // ✅ 1. RENDERIZAR PISO (solo 1 elemento)
            RenderFloor(level.OrderNumber, level.Floor);
            
            // ✅ 2. RENDERIZAR PLATAFORMAS (crear una vez)
            RenderPlatforms(level.OrderNumber, level.Platforms);
            
            // ✅ 3. RENDERIZAR NPCs (crear una vez)
            RenderNPCs(level.NPCs);
            
            // ✅ 4. RENDERIZAR PORTAL (crear una vez)
            RenderPortal();
            
            // ✅ 5. RENDERIZAR JUGADOR (crear una vez)
            RenderPlayer();
            
            _levelRendered = true;
            Console.WriteLine($"✅ Nivel {level.OrderNumber} renderizado con {_gameCanvas.Children.Count} elementos");
        }

        // ✅ RENDERIZAR PISO (solo 1 imagen grande)
        private void RenderFloor(int levelNumber, Level.Platform? floor)
        {
            if (floor == null) return;
            
            Bitmap? floorSprite = GetFloorSprite(levelNumber);
            
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
                // Fallback: rectángulo
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

        // ✅ RENDERIZAR PLATAFORMAS (crear una vez)
        private void RenderPlatforms(int levelNumber, List<Level.Platform> platforms)
        {
            if (platforms == null || platforms.Count == 0) return;
            
            Bitmap? platformSprite = GetPlatformSprite(levelNumber);
            
            foreach (var platform in platforms)
            {
                Image img;
                
                if (platformSprite != null)
                {
                    img = new Image
                    {
                        Source = platformSprite,
                        Width = platform.Width,
                        Height = platform.Height,
                        Stretch = Stretch.Fill
                    };
                }
                else
                {
                    // Fallback: usar Rectangle convertido a Image
                    var rect = new Avalonia.Controls.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(GetLevelColor(levelNumber)),
                        Width = platform.Width,
                        Height = platform.Height
                    };
                    
                    Canvas.SetLeft(rect, platform.X);
                    Canvas.SetTop(rect, platform.Y);
                    _gameCanvas!.Children.Add(rect);
                    continue;
                }
                
                Canvas.SetLeft(img, platform.X);
                Canvas.SetTop(img, platform.Y);
                _gameCanvas!.Children.Add(img);
                _platformImages.Add(img);
            }
        }

        // ✅ RENDERIZAR NPCs (crear una vez)
        private void RenderNPCs(List<Models.NPC> npcs)
        {
            if (npcs == null || npcs.Count == 0) return;
            
            foreach (var npc in npcs)
            {
                Bitmap? npcSprite = GetNPCSpriteByName(npc.Name);
                
                Image img;
                
                if (npcSprite != null)
                {
                    img = new Image
                    {
                        Source = npcSprite,
                        Width = 32,
                        Height = 32
                    };
                }
                else
                {
                    // Fallback: elipse
                    var ellipse = new Avalonia.Controls.Shapes.Ellipse
                    {
                        Fill = new SolidColorBrush(Color.Parse("#00d9ff")),
                        Width = 40,
                        Height = 40
                    };
                    
                    Canvas.SetLeft(ellipse, npc.PositionX - 20);
                    Canvas.SetTop(ellipse, npc.PositionY - 20);
                    _gameCanvas!.Children.Add(ellipse);
                    continue;
                }
                
                Canvas.SetLeft(img, npc.PositionX - 16);
                Canvas.SetTop(img, npc.PositionY - 32);
                _gameCanvas!.Children.Add(img);
                _npcImages.Add(img);
                
                // Nombre del NPC
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

        // ✅ RENDERIZAR PORTAL (crear una vez)
        private void RenderPortal()
        {
            // Efecto de brillo
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
            
            // Portal
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

        // ✅ RENDERIZAR JUGADOR (crear una vez)
        private void RenderPlayer()
        {
            if (_viewModel?.Player == null) return;
            
            if (_playerBitmap != null)
            {
                _playerImage = new Image
                {
                    Source = _playerBitmap,
                    Width = 32,
                    Height = 32
                };
                
                Canvas.SetLeft(_playerImage, _viewModel.Player.Position.X - 16);
                Canvas.SetTop(_playerImage, _viewModel.Player.Position.Y - 32);
                
                _gameCanvas!.Children.Add(_playerImage);
            }
        }

        // ✅ ACTUALIZAR POSICIÓN DEL JUGADOR (llamado cada frame, pero NO recrea elementos)
        private void UpdatePlayerPosition()
        {
            if (_gameCanvas == null || _viewModel?.Player == null || _playerImage == null)
                return;

            // ✅ SOLO actualizar posición y dirección
            Canvas.SetLeft(_playerImage, _viewModel.Player.Position.X - 16);
            Canvas.SetTop(_playerImage, _viewModel.Player.Position.Y - 32);
            
            // Actualizar dirección
            if (_viewModel.Player.IsFacingRight)
            {
                if (_playerImage.RenderTransform != null)
                    _playerImage.RenderTransform = null;
            }
            else
            {
                if (!(_playerImage.RenderTransform is ScaleTransform))
                {
                    _playerImage.RenderTransform = new ScaleTransform(-1, 1);
                    _playerImage.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                }
            }
        }

        // ✅ OBTENER SPRITE DEL PISO (con caché) - Sprites diferentes a las plataformas
        private Bitmap? GetFloorSprite(int levelNumber)
        {
            if (_floorSpriteCache.TryGetValue(levelNumber, out var cachedSprite))
                return cachedSprite;
            
            // Mapeo específico de pisos por nivel (diferentes a las plataformas)
            string floorFile = levelNumber switch
            {
                1 => "pastopiso.png",      // Tutorial - Pasto
                2 => "piedraspiso.png",    // Ruinas - Piedras
                3 => "gobierno.png",       // Ciudad - Gobierno
                4 => "nievepiso.png",      // Laberinto - Nieve
                5 => "piedratutorial.png", // Santuario - Piedra tutorial
                6 => "redlinepiso.png",    // Torre Redline - Redline
                7 => "piedraspiso.png",    // Archivo - Piedras
                _ => "pastopiso.png"
            };
            
            var sprite = LoadBitmap(floorFile);
            _floorSpriteCache[levelNumber] = sprite;
            return sprite;
        }

        // ✅ OBTENER SPRITE DE PLATAFORMA (con caché) - Sprites diferentes al piso
        private Bitmap? GetPlatformSprite(int levelNumber)
        {
            if (_platformSpriteCache.TryGetValue(levelNumber, out var cachedSprite))
                return cachedSprite;
            
            // Mapeo específico de plataformas por nivel (diferentes al piso)
            string platformFile = levelNumber switch
            {
                1 => "platform1.png", // Tutorial - Piedra
                2 => "platform2.png",      // Ruinas - Pasto
                3 => "platform3.png",    // Ciudad - Piedras
                4 => "platform4.png",      // Laberinto - Nieve
                5 => "platform5.png",    // Santuario - Redline
                6 => "platform6.png",       // Torre - Gobierno
                7 => "platform7.png", // Archivo - Piedra tutorial
                _ => "platform1.png"
            };
            
            var sprite = LoadBitmap(platformFile);
            _platformSpriteCache[levelNumber] = sprite;
            return sprite;
        }

        private Bitmap? GetNPCSpriteByName(string npcName)
        {
            if (_npcBitmaps.Count == 0) return null;
            
            string cleanName = npcName.Split('—')[0].Trim().ToLower();
            
            if (cleanName.Contains("custodio") || cleanName.Contains("reportero") || 
                cleanName.Contains("bibliotecario") || cleanName.Contains("guardián"))
                return _npcBitmaps.Count > 0 ? _npcBitmaps[0] : null;
            else if (cleanName.Contains("decano") || cleanName.Contains("villanueva"))
                return _npcBitmaps.Count > 1 ? _npcBitmaps[1] : null;
            else if (cleanName.Contains("noa") || cleanName.Contains("espectra"))
                return _npcBitmaps.Count > 2 ? _npcBitmaps[2] : null;
            else if (cleanName.Contains("archivero") || cleanName.Contains("julián"))
                return _npcBitmaps.Count > 3 ? _npcBitmaps[3] : null;
            else if (cleanName.Contains("ia") || cleanName.Contains("centinela") || 
                     cleanName.Contains("analista"))
                return _npcBitmaps.Count > 1 ? _npcBitmaps[1] : null;
            
            return _npcBitmaps[0];
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