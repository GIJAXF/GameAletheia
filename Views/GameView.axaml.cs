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
            Console.WriteLine("🎮 GameView inicializado");
            
            LoadImages();
            
            Focusable = true;
            AttachedToVisualTree += GameView_AttachedToVisualTree;
        }

        private void LoadImages()
        {
            Console.WriteLine("Intentando cargar sprites...");
            Console.WriteLine($"Directorio actual: {Directory.GetCurrentDirectory()}");
            
            // Verificar si Assets/Images existe físicamente
            var assetsPath = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Images");
            if (Directory.Exists(assetsPath))
            {
                Console.WriteLine($"Directorio físico encontrado: {assetsPath}");
                var files = Directory.GetFiles(assetsPath);
                Console.WriteLine($"Archivos encontrados: {files.Length}");
                foreach (var file in files)
                {
                    Console.WriteLine($"    - {Path.GetFileName(file)}");
                }
            }
            else
            {
                Console.WriteLine($"Directorio físico NO encontrado: {assetsPath}");
            }
            
            try
            {
                // Cargar jugador según género (se actualizará en RenderLevel)
                // Por defecto cargamos hombre, pero se cambiará dinámicamente
                _playerBitmap = LoadBitmap("playerH.png");
                
                // Intentar cargar NPCs (4 variaciones específicas)
                // npc1 = Aletheia (genéricos)
                // npc2 = Decano Villanueva
                // npc3 = Noa Espectra
                // npc4 = El Archivero
                _npcBitmaps.Add(LoadBitmap("npc1.png")); // Aletheia / Genéricos
                _npcBitmaps.Add(LoadBitmap("npc2.png")); // Decano Villanueva
                _npcBitmaps.Add(LoadBitmap("npc3.png")); // Noa Espectra
                _npcBitmaps.Add(LoadBitmap("npc4.png")); // El Archivero
                
                // Intentar cargar plataformas (7 variaciones)
                _platformBitmaps.Add(LoadBitmap("platform1.png"));
                _platformBitmaps.Add(LoadBitmap("platform2.png"));
                _platformBitmaps.Add(LoadBitmap("platform3.png"));
                _platformBitmaps.Add(LoadBitmap("platform4.png"));
                _platformBitmaps.Add(LoadBitmap("platform5.png"));
                _platformBitmaps.Add(LoadBitmap("platform6.png"));
                _platformBitmaps.Add(LoadBitmap("platform7.png"));
                
                // Portal
                _portalBitmap = LoadBitmap("portal.png");
                
                // Verificar si al menos el jugador se cargó
                _spritesLoaded = _playerBitmap != null;
                
                Console.WriteLine("Resumen de carga de sprites:");
                Console.WriteLine($"   - Jugador: {(_playerBitmap != null ? "Cargado" : "Usando fallback")}");
                Console.WriteLine($"   - NPCs cargados: {_npcBitmaps.Count(b => b != null)}/4");
                Console.WriteLine($"   - Plataformas cargadas: {_platformBitmaps.Count(b => b != null)}/7");
                Console.WriteLine($"   - Portal: {(_portalBitmap != null ? "Cargado" : "Usando fallback")}");
                
                if (!_spritesLoaded)
                {
                    Console.WriteLine("No se cargaron sprites. Se usarán figuras geométricas.");
                    Console.WriteLine("Para usar sprites, coloca archivos PNG en Assets/Images/");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando sprites: {ex.Message}");
                Console.WriteLine("Se usarán figuras geométricas como fallback");
                _spritesLoaded = false;
            }
        }
        
        private Bitmap? LoadBitmap(string filename)
        {
            try
            {
                var uri = new Uri($"avares://GameAletheiaCross/Assets/Images/{filename}");
                Console.WriteLine($"Intentando cargar: {uri}");
                
                var stream = AssetLoader.Open(uri);
                var bitmap = new Bitmap(stream);
                
                Console.WriteLine($"{filename} cargado correctamente");
                return bitmap;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"No se pudo cargar {filename}: {ex.Message}");
                return null;
            }
        }

        private void GameView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Console.WriteLine("GameView attached to visual tree");
            
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
                Console.WriteLine("CRÍTICO: No se pudo encontrar ningún Canvas");
                return;
            }
            
            _viewModel = (GameViewModel?)DataContext;
            
            if (_viewModel == null)
            {
                Console.WriteLine("ERROR: ViewModel es null");
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
    Console.WriteLine("=== RENDERIZANDO NIVEL ===");
    
    if (_gameCanvas == null || _viewModel?.CurrentLevel == null)
    {
        Console.WriteLine("ERROR: Canvas o CurrentLevel es null");
        return;
    }

    var level = _viewModel.CurrentLevel;
    _gameCanvas.Children.Clear(); // ⚠️ IMPORTANTE: Limpiar ANTES de renderizar

    // ACTUALIZAR SPRITE DEL JUGADOR SEGÚN GÉNERO
    if (_viewModel.Player != null)
    {
        string playerSprite = _viewModel.Player.Gender == "Hombre" ? "playerH.png" : "playerM.png";
        _playerBitmap = LoadBitmap(playerSprite);
        
        if (_playerBitmap != null)
        {
            Console.WriteLine($"✅ Sprite de jugador cargado: {playerSprite}");
        }
    }

    // 🟦 RENDERIZAR PLATAFORMAS
    if (level.Platforms != null && level.Platforms.Count > 0)
    {
        Console.WriteLine($"Renderizando {level.Platforms.Count} plataformas...");
        
        for (int i = 0; i < level.Platforms.Count; i++)
        {
            var platform = level.Platforms[i];
            
            Bitmap? platformSprite = GetPlatformSprite(level.OrderNumber, i);
            
            if (platformSprite != null)
            {
                // ⚠️ CREAR NUEVA IMAGEN CADA VEZ
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

    // 👥 RENDERIZAR NPCs
    if (level.NPCs != null && level.NPCs.Count > 0)
    {
        Console.WriteLine($"Renderizando {level.NPCs.Count} NPCs...");
        
        for (int i = 0; i < level.NPCs.Count; i++)
        {
            var npc = level.NPCs[i];
            
            Bitmap? npcSprite = GetNPCSpriteByName(npc.Name);
            
            if (npcSprite != null)
            {
                // ⚠️ CREAR NUEVA IMAGEN CADA VEZ
                var img = new Image
                {
                    Source = npcSprite,
                    Width = 32,
                    Height = 32
                };
                
                Canvas.SetLeft(img, npc.PositionX - 16);
                Canvas.SetTop(img, npc.PositionY - 32);
                _gameCanvas.Children.Add(img);
                
                Console.WriteLine($"✅ Renderizado NPC: {npc.Name.Split('—')[0].Trim()}");
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

    // 🧑 RENDERIZAR JUGADOR
    if (_viewModel.Player != null)
    {
        if (_playerBitmap != null)
        {
            // ⚠️ CREAR NUEVA IMAGEN CADA VEZ
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

    // 🌀 RENDERIZAR PORTAL (VERTICAL)
    if (_portalBitmap != null)
    {
        // Efecto de brillo (fondo)
        var glow = new Avalonia.Controls.Shapes.Ellipse
        {
            Fill = new SolidColorBrush(Color.Parse("#00FF88")),
            Width = 60,
            Height = 120,
            Opacity = 0.3
        };
        
        Canvas.SetLeft(glow, 670);
        Canvas.SetTop(glow, 310);
        _gameCanvas.Children.Add(glow);
        
        // Portal (frente)
        var portalImg = new Image
        {
            Source = _portalBitmap,
            Width = 40,
            Height = 100,
            Opacity = 0.8,
            Stretch = Stretch.Fill
        };
        
        Canvas.SetLeft(portalImg, 680);
        Canvas.SetTop(portalImg, 320);
        _gameCanvas.Children.Add(portalImg);
    }
    else
    {
        // Fallback: elipse verde vertical
        var portal = new Avalonia.Controls.Shapes.Ellipse
        {
            Fill = new SolidColorBrush(Color.Parse("#00FF88")),
            Stroke = new SolidColorBrush(Color.Parse("#00FF00")),
            StrokeThickness = 2,
            Width = 40,
            Height = 100,
            Opacity = 0.7
        };
        
        Canvas.SetLeft(portal, 680);
        Canvas.SetTop(portal, 320);
        _gameCanvas.Children.Add(portal);
    }

    Console.WriteLine($"✅ Nivel renderizado. Total: {_gameCanvas.Children.Count} elementos");
}

// Reemplaza el método GetPlatformSprite en GameView.axaml.cs

            private Bitmap? GetPlatformSprite(int levelNumber, int platformIndex)
            {
                // Mapeo de niveles a pisos específicos
                string platformFile = levelNumber switch
                {
                    1 => "piedratutorial.png",  // Nivel 1: Tutorial
                    2 => "pastopiso.png",        // Nivel 2: Ruinas del Firewall
                    3 => "piedraspiso.png",      // Nivel 3: Ciudad de Contraseñas
                    4 => "nievepiso.png",        // Nivel 4: Laberinto de Algoritmos
                    5 => "redlinepiso.png",      // Nivel 5: Santuario de Datos
                    6 => "gobierno.png",         // Nivel 6: Torre Corporativa
                    7 => "piedraspiso.png",      // Nivel 7: Archivo Prohibido (reutilizar)
                    _ => "platform1.png"         // Fallback
                };
                
                try
                {
                    var uri = new Uri($"avares://GameAletheiaCross/Assets/Images/{platformFile}");
                    using var stream = AssetLoader.Open(uri);
                    return new Bitmap(stream);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"⚠️ No se pudo cargar {platformFile}: {ex.Message}");
                    
                    // Intentar usar los platform genericos como fallback
                    if (_platformBitmaps.Count > 0)
                    {
                        int spriteIndex = (levelNumber - 1) % _platformBitmaps.Count;
                        return _platformBitmaps[spriteIndex];
                    }
                    
                    return null;
                }
            }

        private Bitmap? GetNPCSprite(int npcIndex)
        {
            if (_npcBitmaps.Count == 0) return null;
            
            int spriteIndex = npcIndex % _npcBitmaps.Count;
            return _npcBitmaps[spriteIndex];
        }

        /// <summary>
        /// Asigna el sprite correcto según el nombre del NPC
        /// </summary>
        private Bitmap? GetNPCSpriteByName(string npcName)
        {
            if (_npcBitmaps.Count == 0) return null;
            
            // Limpiar el nombre (quitar lo que está después del guión)
            string cleanName = npcName.Split('—')[0].Trim().ToLower();
            
            // Mapeo de nombres a índices de sprites
            // npc1.png = Aletheia / Genéricos (índice 0)
            // npc2.png = Decano Villanueva (índice 1)
            // npc3.png = Noa Espectra (índice 2)
            // npc4.png = El Archivero (índice 3)
            
            if (cleanName.Contains("custodio") || cleanName.Contains("reportero") || 
                cleanName.Contains("bibliotecario") || cleanName.Contains("guardián"))
            {
                // NPCs genéricos de la Resistencia/Gobierno usan sprite 1
                return _npcBitmaps.Count > 0 ? _npcBitmaps[0] : null;
            }
            else if (cleanName.Contains("decano") || cleanName.Contains("villanueva"))
            {
                // Decano Villanueva usa sprite 2
                return _npcBitmaps.Count > 1 ? _npcBitmaps[1] : null;
            }
            else if (cleanName.Contains("noa") || cleanName.Contains("espectra"))
            {
                // Noa Espectra usa sprite 3
                return _npcBitmaps.Count > 2 ? _npcBitmaps[2] : null;
            }
            else if (cleanName.Contains("archivero") || cleanName.Contains("julián") || 
                     cleanName.Contains("casablancas"))
            {
                // El Archivero usa sprite 4
                return _npcBitmaps.Count > 3 ? _npcBitmaps[3] : null;
            }
            else if (cleanName.Contains("ia") || cleanName.Contains("centinela") || 
                     cleanName.Contains("analista") || cleanName.Contains("fractal"))
            {
                // NPCs de Redline usan sprite 2
                return _npcBitmaps.Count > 1 ? _npcBitmaps[1] : null;
            }
            
            // Por defecto usar el primer sprite
            return _npcBitmaps[0];
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