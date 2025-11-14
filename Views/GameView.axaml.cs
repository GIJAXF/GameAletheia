using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GameAletheiaCross.Models;
using GameAletheiaCross.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace GameAletheiaCross.Views
{
    public partial class GameView : UserControl
    {
        private Canvas? _gameCanvas;
        private GameViewModel? _viewModel;
        private Image? _playerImage;
        
        // Cache de imágenes
        private Bitmap? _playerBitmap;
        private List<Bitmap?> _npcBitmaps = new();
        private List<Bitmap?> _platformBitmaps = new();
        private Bitmap? _portalBitmap;
        
        private bool _spritesLoaded = false;

        public GameView()
        {
            InitializeComponent();
            Console.WriteLine("  GameView inicializado");
            
            LoadImages();
            
            Focusable = true;
            AttachedToVisualTree += GameView_AttachedToVisualTree;
        }

        private void LoadImages()
        {
            Console.WriteLine("  Intentando cargar sprites...");
            Console.WriteLine($"  Directorio actual: {Directory.GetCurrentDirectory()}");
            
            // Verificar si Assets/Images existe físicamente
            var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images");
            if (Directory.Exists(assetsPath))
            {
                Console.WriteLine($"  Directorio físico encontrado: {assetsPath}");
                var files = Directory.GetFiles(assetsPath);
                Console.WriteLine($"  Archivos encontrados: {files.Length}");
                foreach (var file in files)
                {
                    Console.WriteLine($"    - {Path.GetFileName(file)}");
                }
            }
            else
            {
                Console.WriteLine($" ️ Directorio físico NO encontrado: {assetsPath}");
            }
            
            try
            {
                // Intentar cargar jugador
                _playerBitmap = LoadBitmap("playerH.png");
                
                // Intentar cargar NPCs (3 variaciones)
                _npcBitmaps.Add(LoadBitmap("npc1.png"));
                _npcBitmaps.Add(LoadBitmap("npc2.png"));
                _npcBitmaps.Add(LoadBitmap("npc3.png"));
                
                // Intentar cargar plataformas (7 variaciones)
                _platformBitmaps.Add(LoadBitmap("platform1.png"));
                _platformBitmaps.Add(LoadBitmap("platform2.png"));
                _platformBitmaps.Add(LoadBitmap("platform3.png"));
                _platformBitmaps.Add(LoadBitmap("platform4.png"));
                _platformBitmaps.Add(LoadBitmap("platform5.png"));
                _platformBitmaps.Add(LoadBitmap("platform6.png"));
                _platformBitmaps.Add(LoadBitmap("platform7.png"));
                
                // Portal
                _portalBitmap = LoadBitmap("platform7.png");
                
                // Verificar si al menos el jugador se cargó
                _spritesLoaded = _playerBitmap != null;
                
                Console.WriteLine("  Resumen de carga de sprites:");
                Console.WriteLine($"   - Jugador: {(_playerBitmap != null ? "  Cargado" : "  Usando fallback")}");
                Console.WriteLine($"   - NPCs cargados: {_npcBitmaps.Count(b => b != null)}/3");
                Console.WriteLine($"   - Plataformas cargadas: {_platformBitmaps.Count(b => b != null)}/7");
                Console.WriteLine($"   - Portal: {(_portalBitmap != null ? "  Cargado" : "  Usando fallback")}");
                
                if (!_spritesLoaded)
                {
                    Console.WriteLine(" ️ No se cargaron sprites. Se usarán figuras geométricas.");
                    Console.WriteLine("   Para usar sprites, coloca archivos PNG en Assets/Images/");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($" ️ Error cargando sprites: {ex.Message}");
                Console.WriteLine("   Se usarán figuras geométricas como fallback");
                _spritesLoaded = false;
            }
        }
        
        private Bitmap? LoadBitmap(string filename)
        {
            try
            {
                var uri = new Uri($"avares://GameAletheiaCross/Assets/Images/{filename}");
                Console.WriteLine($"   Intentando cargar: {uri}");
                
                var stream = AssetLoader.Open(uri);
                var bitmap = new Bitmap(stream);
                
                Console.WriteLine($"     {filename} cargado correctamente");
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"     No se pudo cargar {filename}: {ex.Message}");
                return null;
            }
        }

        private void GameView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Console.WriteLine("  GameView attached to visual tree");
            
            Focus();
            
            _gameCanvas = this.FindControl<Canvas>("GameCanvas");
            
            if (_gameCanvas == null)
            {
                foreach (var child in this.GetVisualDescendants())
                {
                    if (child is Canvas c)
                    {
                        _gameCanvas = c;
                        break;
                    }
                }
            }
            
            if (_gameCanvas == null)
            {
                Console.WriteLine("  CRÍTICO: No se pudo encontrar ningún Canvas");
                return;
            }
            
            _viewModel = (GameViewModel?)DataContext;
            
            if (_viewModel == null)
            {
                Console.WriteLine("  ERROR: ViewModel es null");
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

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GameViewModel.CurrentLevel))
            {
                Dispatcher.UIThread.Post(RenderLevel);
            }
            else if (e.PropertyName == nameof(GameViewModel.Player))
            {
                Dispatcher.UIThread.Post(UpdatePlayerPosition);
            }
        }

        private void RenderLevel()
        {
            Console.WriteLine("  === RENDERIZANDO NIVEL ===");
            
            if (_gameCanvas == null || _viewModel?.CurrentLevel == null)
            {
                Console.WriteLine("  ERROR: Canvas o CurrentLevel es null");
                return;
            }

            var level = _viewModel.CurrentLevel;
            _gameCanvas.Children.Clear();

            //  ️ RENDERIZAR PLATAFORMAS
            if (level.Platforms != null && level.Platforms.Count > 0)
            {
                Console.WriteLine($" ️ Renderizando {level.Platforms.Count} plataformas...");
                
                for (int i = 0; i < level.Platforms.Count; i++)
                {
                    var platform = level.Platforms[i];
                    
                    Bitmap? platformSprite = GetPlatformSprite(level.OrderNumber, i);
                    
                    if (platformSprite != null)
                    {
                        // Usar sprite
                        var img = new Image
                        {
                            Source = platformSprite,
                            Width = platform.Width,
                            Height = platform.Height,
                            Stretch = Stretch.Fill
                        };
                        
                        Canvas.SetLeft(img, platform.X);
                        Canvas.SetTop(img, platform.Y);
                        _gameCanvas.Children.Add(img);
                    }
                    else
                    {
                        // Fallback: rectángulo con color
                        var color = GetLevelColor(level.OrderNumber);
                        var rect = new Avalonia.Controls.Shapes.Rectangle
                        {
                            Fill = new SolidColorBrush(color),
                            Stroke = new SolidColorBrush(Colors.White),
                            StrokeThickness = 2,
                            Width = platform.Width,
                            Height = platform.Height
                        };
                        
                        Canvas.SetLeft(rect, platform.X);
                        Canvas.SetTop(rect, platform.Y);
                        _gameCanvas.Children.Add(rect);
                    }
                }
            }

            //   RENDERIZAR NPCs
            if (level.NPCs != null && level.NPCs.Count > 0)
            {
                Console.WriteLine($"  Renderizando {level.NPCs.Count} NPCs...");
                
                for (int i = 0; i < level.NPCs.Count; i++)
                {
                    var npc = level.NPCs[i];
                    
                    Bitmap? npcSprite = GetNPCSprite(i);
                    
                    if (npcSprite != null)
                    {
                        // Usar sprite
                        var img = new Image
                        {
                            Source = npcSprite,
                            Width = 32,
                            Height = 32
                        };
                        
                        Canvas.SetLeft(img, npc.PositionX - 16);
                        Canvas.SetTop(img, npc.PositionY - 32);
                        _gameCanvas.Children.Add(img);
                    }
                    else
                    {
                        // Fallback: elipse azul
                        var ellipse = new Avalonia.Controls.Shapes.Ellipse
                        {
                            Fill = new SolidColorBrush(Color.Parse("#00d9ff")),
                            Stroke = new SolidColorBrush(Color.Parse("#0080FF")),
                            StrokeThickness = 1,
                            Width = 40,
                            Height = 40
                        };
                        
                        Canvas.SetLeft(ellipse, npc.PositionX - 20);
                        Canvas.SetTop(ellipse, npc.PositionY - 20);
                        _gameCanvas.Children.Add(ellipse);
                    }
                    
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
                    _gameCanvas.Children.Add(nameText);
                }
            }

            //   RENDERIZAR JUGADOR
            if (_viewModel.Player != null)
            {
                if (_playerBitmap != null)
                {
                    // Usar sprite
                    _playerImage = new Image
                    {
                        Source = _playerBitmap,
                        Width = 32,
                        Height = 32
                    };
                    
                    Canvas.SetLeft(_playerImage, _viewModel.Player.Position.X - 16);
                    Canvas.SetTop(_playerImage, _viewModel.Player.Position.Y - 32);
                    
                    if (!_viewModel.Player.IsFacingRight)
                    {
                        _playerImage.RenderTransform = new ScaleTransform(-1, 1);
                        _playerImage.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                    }
                    
                    _gameCanvas.Children.Add(_playerImage);
                }
                else
                {
                    // Fallback: rectángulo rojo
                    var rect = new Avalonia.Controls.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(Color.Parse("#e94560")),
                        Stroke = new SolidColorBrush(Color.Parse("#ff0080")),
                        StrokeThickness = 2,
                        Width = 40,
                        Height = 60
                    };
                    
                    Canvas.SetLeft(rect, _viewModel.Player.Position.X - 20);
                    Canvas.SetTop(rect, _viewModel.Player.Position.Y - 60);
                    _gameCanvas.Children.Add(rect);
                }
            }

            //   RENDERIZAR PORTAL (VERTICAL)
            if (_portalBitmap != null)
            {
                var img = new Image
                {
                    Source = _portalBitmap,
                    Width = 100,  // Ancho temporal (antes de rotar)
                    Height = 40,  // Alto temporal (antes de rotar)
                    Opacity = 0.7,
                    RenderTransform = new RotateTransform(90), // Rotar 90° para hacerlo vertical
                    RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative)
                };
                
                Canvas.SetLeft(img, 680);
                Canvas.SetTop(img, 340);
                _gameCanvas.Children.Add(img);
            }
            else
            {
                // Fallback: rectángulo rojo vertical
                var portal = new Avalonia.Controls.Shapes.Rectangle
                {
                    Fill = new SolidColorBrush(Color.Parse("#E63946")),
                    Stroke = new SolidColorBrush(Color.Parse("#FF0000")),
                    StrokeThickness = 2,
                    Width = 40,   // Estrecho
                    Height = 100, // Alto (vertical)
                    Opacity = 0.6
                };
                
                Canvas.SetLeft(portal, 680);
                Canvas.SetTop(portal, 320);
                _gameCanvas.Children.Add(portal);
            }

            Console.WriteLine($"  Nivel renderizado. Total: {_gameCanvas.Children.Count} elementos");
        }

        private Bitmap? GetPlatformSprite(int levelNumber, int platformIndex)
        {
            if (_platformBitmaps.Count == 0) return null;
            
            int spriteIndex = (levelNumber - 1) % _platformBitmaps.Count;
            return _platformBitmaps[spriteIndex];
        }

        private Bitmap? GetNPCSprite(int npcIndex)
        {
            if (_npcBitmaps.Count == 0) return null;
            
            int spriteIndex = npcIndex % _npcBitmaps.Count;
            return _npcBitmaps[spriteIndex];
        }

        private Color GetLevelColor(int levelNumber)
        {
            return levelNumber switch
            {
                1 => Color.Parse("#9D4EDD"), // Morado
                2 => Color.Parse("#06D6A0"), // Verde
                3 => Color.Parse("#FF9E00"), // Naranja
                4 => Color.Parse("#00D9FF"), // Cyan
                5 => Color.Parse("#0080FF"), // Azul
                6 => Color.Parse("#FFD60A"), // Amarillo
                7 => Color.Parse("#E63946"), // Rojo
                _ => Color.Parse("#FFFFFF")
            };
        }

        private void UpdatePlayerPosition()
        {
            if (_gameCanvas == null || _viewModel?.Player == null)
                return;

            if (_playerImage != null)
            {
                Canvas.SetLeft(_playerImage, _viewModel.Player.Position.X - 16);
                Canvas.SetTop(_playerImage, _viewModel.Player.Position.Y - 32);
                
                if (_viewModel.Player.IsFacingRight)
                {
                    _playerImage.RenderTransform = null;
                }
                else
                {
                    _playerImage.RenderTransform = new ScaleTransform(-1, 1);
                    _playerImage.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                }
            }
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