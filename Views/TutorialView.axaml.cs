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
using System.IO;
using System.Linq;

namespace GameAletheiaCross.Views
{
    public partial class TutorialView : UserControl
    {
        private Canvas? _tutorialCanvas;
        private TutorialViewModel? _viewModel;
        private Image? _playerImage;
        
        // Cache de sprites
        private Bitmap? _playerBitmap;
        private Bitmap? _platformBitmap;
        private Bitmap? _portalBitmap;
        private Bitmap? _floorBitmap;
        private Bitmap? _backgroundBitmap; // 🆕 FONDO DEL TUTORIAL
        
        private bool _spritesLoaded = false;

        public TutorialView()
        {
            InitializeComponent();
            Console.WriteLine(" TutorialView inicializado");
            
            LoadSprites();
            
            Focusable = true;
            AttachedToVisualTree += TutorialView_AttachedToVisualTree;
        }

        private void LoadSprites()
        {
            Console.WriteLine(" Cargando sprites del tutorial...");
            
            try
            {
                // 🆕 Cargar fondo del tutorial (forest)
                _backgroundBitmap = LoadBitmap("ForestFondo.png");
                
                // Cargar jugador (se actualizará según género)
                _playerBitmap = LoadBitmap("playerH.png");
                
                // Plataforma del tutorial
                _platformBitmap = LoadBitmap("platform1.png");
                
                // Portal
                _portalBitmap = LoadBitmap("Portal.png");
                
                // Piso del tutorial
                _floorBitmap = LoadBitmap("PastoPiso.png");
                
                _spritesLoaded = _playerBitmap != null;
                
                Console.WriteLine(" Sprites del tutorial:");
                Console.WriteLine($"   - Fondo: {(_backgroundBitmap != null ? "" : "")}");
                Console.WriteLine($"   - Jugador: {(_playerBitmap != null ? "" : "")}");
                Console.WriteLine($"   - Plataforma: {(_platformBitmap != null ? "" : "")}");
                Console.WriteLine($"   - Portal: {(_portalBitmap != null ? "" : "")}");
                Console.WriteLine($"   - Piso: {(_floorBitmap != null ? "" : "")}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error cargando sprites: {ex.Message}");
                _spritesLoaded = false;
            }
        }
        
        private Bitmap? LoadBitmap(string filename)
        {
            try
            {
                var uri = new Uri($"avares://GameAletheiaCross/Assets/Images/{filename}");
                var stream = AssetLoader.Open(uri);
                var bitmap = new Bitmap(stream);
                Console.WriteLine($" {filename} cargado correctamente");
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($" No se pudo cargar {filename}: {ex.Message}");
                return null;
            }
        }

        private void TutorialView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Console.WriteLine(" TutorialView attached to visual tree");
            
            Focus();
            
            _tutorialCanvas = this.FindControl<Canvas>("TutorialCanvas");
            
            if (_tutorialCanvas == null)
            {
                foreach (var child in this.GetVisualDescendants())
                {
                    if (child is Canvas c)
                    {
                        _tutorialCanvas = c;
                        break;
                    }
                }
            }
            
            if (_tutorialCanvas == null)
            {
                Console.WriteLine(" CRÍTICO: No se pudo encontrar TutorialCanvas");
                return;
            }
            
            _viewModel = (TutorialViewModel?)DataContext;
            
            if (_viewModel == null)
            {
                Console.WriteLine(" ERROR: ViewModel es null");
                return;
            }
            
            // Actualizar sprite del jugador según género
            if (_viewModel.Player != null)
            {
                string playerSprite = _viewModel.Player.Gender == "Hombre" ? "playerH.png" : "playerM.png";
                _playerBitmap = LoadBitmap(playerSprite);
                Console.WriteLine($" Sprite cargado: {playerSprite}");
            }
            
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            Dispatcher.UIThread.Post(RenderTutorial);
            
            Console.WriteLine(" TutorialView configurado correctamente");
            
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TutorialViewModel.Player))
            {
                Dispatcher.UIThread.Post(UpdatePlayerPosition);
            }
        }

        private void RenderTutorial()
        {
            Console.WriteLine("=== RENDERIZANDO TUTORIAL ===");
            
            if (_tutorialCanvas == null || _viewModel?.Player == null)
            {
                Console.WriteLine(" ERROR: Canvas o Player es null");
                return;
            }

            _tutorialCanvas.Children.Clear();

            // 🆕 RENDERIZAR FONDO DEL TUTORIAL (DEBE SER LO PRIMERO)
            if (_backgroundBitmap != null)
            {
                var backgroundImg = new Image
                {
                    Source = _backgroundBitmap,
                    Width = 1280,
                    Height = 720,
                    Stretch = Stretch.UniformToFill
                };
                
                Canvas.SetLeft(backgroundImg, 0);
                Canvas.SetTop(backgroundImg, 0);
                _tutorialCanvas.Children.Add(backgroundImg);
                
                Console.WriteLine("️ Fondo del tutorial renderizado (forest)");
            }
            else
            {
                // Fallback: fondo verde bosque
                var fallbackRect = new Avalonia.Controls.Shapes.Rectangle
                {
                    Fill = new SolidColorBrush(Color.Parse("#2d4a2b")), // Verde oscuro bosque
                    Width = 1280,
                    Height = 720,
                    Opacity = 0.8
                };
                
                Canvas.SetLeft(fallbackRect, 0);
                Canvas.SetTop(fallbackRect, 0);
                _tutorialCanvas.Children.Add(fallbackRect);
                
                Console.WriteLine("️ Usando fondo fallback para tutorial");
            }

            // RENDERIZAR PISO
            if (_viewModel.TutorialPlatforms.Count > 0)
            {
                var floor = _viewModel.TutorialPlatforms[0]; // El piso es la primera plataforma
                
                if (_floorBitmap != null)
                {
                    var floorImg = new Image
                    {
                        Source = _floorBitmap,
                        Width = floor.Width,
                        Height = floor.Height,
                        Stretch = Stretch.Fill
                    };
                    
                    Canvas.SetLeft(floorImg, floor.X);
                    Canvas.SetTop(floorImg, floor.Y);
                    _tutorialCanvas.Children.Add(floorImg);
                    
                    Console.WriteLine(" Piso del tutorial renderizado");
                }
                else
                {
                    var rect = new Avalonia.Controls.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(Color.Parse("#2a2a2a")),
                        Stroke = new SolidColorBrush(Colors.Gray),
                        StrokeThickness = 2,
                        Width = floor.Width,
                        Height = floor.Height
                    };
                    
                    Canvas.SetLeft(rect, floor.X);
                    Canvas.SetTop(rect, floor.Y);
                    _tutorialCanvas.Children.Add(rect);
                }
            }

            // RENDERIZAR PLATAFORMAS (excepto el piso)
            for (int i = 1; i < _viewModel.TutorialPlatforms.Count; i++)
            {
                var platform = _viewModel.TutorialPlatforms[i];
                
                if (_platformBitmap != null)
                {
                    var img = new Image
                    {
                        Source = _platformBitmap,
                        Width = platform.Width,
                        Height = platform.Height,
                        Stretch = Stretch.Fill
                    };
                    
                    Canvas.SetLeft(img, platform.X);
                    Canvas.SetTop(img, platform.Y);
                    _tutorialCanvas.Children.Add(img);
                }
                else
                {
                    var rect = new Avalonia.Controls.Shapes.Rectangle
                    {
                        Fill = new SolidColorBrush(Color.Parse("#4A90E2")),
                        Stroke = new SolidColorBrush(Color.Parse("#00ff88")),
                        StrokeThickness = 2,
                        Width = platform.Width,
                        Height = platform.Height
                    };
                    
                    Canvas.SetLeft(rect, platform.X);
                    Canvas.SetTop(rect, platform.Y);
                    _tutorialCanvas.Children.Add(rect);
                }
            }

            // RENDERIZAR JUGADOR
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
                
                if (!_viewModel.Player.IsFacingRight)
                {
                    _playerImage.RenderTransform = new ScaleTransform(-1, 1);
                    _playerImage.RenderTransformOrigin = new RelativePoint(0.5, 0.5, RelativeUnit.Relative);
                }
                
                _tutorialCanvas.Children.Add(_playerImage);
            }
            else
            {
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
                _tutorialCanvas.Children.Add(rect);
            }

            // RENDERIZAR PORTAL (META)
            if (_portalBitmap != null)
            {
                var portalImg = new Image
                {
                    Source = _portalBitmap,
                    Width = 60,
                    Height = 120,
                    Opacity = 0.8,
                    Stretch = Stretch.Uniform
                };
                
                Canvas.SetLeft(portalImg, 970); // Centro de la plataforma (900 + 100 - 30)
                Canvas.SetTop(portalImg, 280);  // Justo sobre la plataforma (400 - 120)
                _tutorialCanvas.Children.Add(portalImg);
            }
            else
            {
                var portal = new Avalonia.Controls.Shapes.Rectangle
                {
                    Fill = new SolidColorBrush(Color.Parse("#00ff88")),
                    Stroke = new SolidColorBrush(Color.Parse("#00ff00")),
                    StrokeThickness = 3,
                    Width = 80,
                    Height = 100,
                    Opacity = 0.7
                };
                
                Canvas.SetLeft(portal, 960);  // Centro de la plataforma
                Canvas.SetTop(portal, 300);   // Justo sobre la plataforma (400 - 100)
                _tutorialCanvas.Children.Add(portal);
            }

            // INDICADOR DE META (TEXTO)
            var metaText = new TextBlock
            {
                Text = "META",
                FontSize = 14,
                FontWeight = FontWeight.Bold,
                Foreground = new SolidColorBrush(Color.Parse("#ffffff")),
                Background = new SolidColorBrush(Color.FromArgb(170, 0, 0, 0)),
                Padding = new Thickness(8, 4)
            };
            
            Canvas.SetLeft(metaText, 975);  // Alineado con el portal
            Canvas.SetTop(metaText, 250);   // Justo encima del portal
            _tutorialCanvas.Children.Add(metaText);

            Console.WriteLine($" Tutorial renderizado. Total: {_tutorialCanvas.Children.Count} elementos");
        }

        private void UpdatePlayerPosition()
        {
            if (_tutorialCanvas == null || _viewModel?.Player == null)
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
            if (_viewModel == null) return;

            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    _viewModel.KeyLeft = true;
                    break;

                case Key.D:
                case Key.Right:
                    _viewModel.KeyRight = true;
                    break;

                case Key.W:
                case Key.Up:
                    _viewModel.KeyUp = true;
                    break;
            }
        }

        private void OnKeyUp(object? sender, KeyEventArgs e)
        {
            if (_viewModel == null) return;

            switch (e.Key)
            {
                case Key.A:
                case Key.Left:
                    _viewModel.KeyLeft = false;
                    break;

                case Key.D:
                case Key.Right:
                    _viewModel.KeyRight = false;
                    break;

                case Key.W:
                case Key.Up:
                    _viewModel.KeyUp = false;
                    break;
            }
        }
    }
}