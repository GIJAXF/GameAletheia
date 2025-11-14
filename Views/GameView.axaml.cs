using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using GameAletheiaCross.Models;
using GameAletheiaCross.ViewModels;
using System;

namespace GameAletheiaCross.Views
{
    public partial class GameView : UserControl
    {
        private Canvas? _gameCanvas;
        private GameViewModel? _viewModel;
        private Shape? _playerShape;

        public GameView()
        {
            InitializeComponent();
            Console.WriteLine("🎮 GameView inicializado");
            
            Focusable = true;
            AttachedToVisualTree += GameView_AttachedToVisualTree;
        }

        private void GameView_AttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        {
            Console.WriteLine("🎮 GameView attached to visual tree");
            
            Focus();
            
            //  ENCONTRAR EL CANVAS POR NOMBRE
            _gameCanvas = this.FindControl<Canvas>("GameCanvas");
            
            if (_gameCanvas == null)
            {
                Console.WriteLine(" ERROR: Canvas 'GameCanvas' no encontrado");
                Console.WriteLine("   Buscando todos los Canvas disponibles...");
                
                // Buscar recursivamente cualquier Canvas
                foreach (var child in this.GetVisualDescendants())
                {
                    if (child is Canvas c)
                    {
                        Console.WriteLine($"   ✓ Canvas encontrado: {c.Name ?? "sin nombre"}");
                        if (_gameCanvas == null && c.Name == null)
                        {
                            _gameCanvas = c;
                            Console.WriteLine("    Usando este Canvas");
                            break;
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine(" GameCanvas encontrado por nombre");
            }
            
            if (_gameCanvas == null)
            {
                Console.WriteLine(" CRÍTICO: No se pudo encontrar ningún Canvas");
                return;
            }
            
            _viewModel = (GameViewModel?)DataContext;
            
            if (_viewModel == null)
            {
                Console.WriteLine(" ERROR: ViewModel es null");
                return;
            }
            
            Console.WriteLine(" ViewModel encontrado");
            
            // 🎯 SUSCRIBIRSE A CAMBIOS DEL VIEWMODEL
            _viewModel.PropertyChanged += ViewModel_PropertyChanged;
            
            Console.WriteLine($" PropertyChanged suscrito. CurrentLevel: {_viewModel.CurrentLevel?.Name ?? "null"}");
            
            // 🎯 RENDERIZAR INMEDIATAMENTE SI YA HAY NIVEL
            if (_viewModel.CurrentLevel != null)
            {
                Console.WriteLine(" CurrentLevel ya existe, renderizando inmediatamente...");
                Dispatcher.UIThread.Post(() =>
                {
                    Console.WriteLine("📍 Dispatcher ejecutando RenderLevel");
                    RenderLevel();
                });
            }
            else
            {
                Console.WriteLine("⏳ CurrentLevel es null, esperando cambios...");
            }
            
            AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
            AddHandler(KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            Console.WriteLine($"📢 PropertyChanged: {e.PropertyName}");
            
            if (e.PropertyName == nameof(GameViewModel.CurrentLevel))
            {
                Console.WriteLine("🎮 CurrentLevel cambió, renderizando...");
                Dispatcher.UIThread.Post(() =>
                {
                    Console.WriteLine("📍 Dispatcher ejecutando RenderLevel desde PropertyChanged");
                    RenderLevel();
                });
            }
            else if (e.PropertyName == nameof(GameViewModel.Player))
            {
                Dispatcher.UIThread.Post(UpdatePlayerPosition);
            }
        }

        private void RenderLevel()
        {
            Console.WriteLine("🎨 === RENDERIZANDO NIVEL ===");
            
            if (_gameCanvas == null)
            {
                Console.WriteLine(" ERROR: _gameCanvas es null");
                return;
            }

            if (_viewModel?.CurrentLevel == null)
            {
                Console.WriteLine(" ERROR: CurrentLevel es null");
                return;
            }

            var level = _viewModel.CurrentLevel;
            Console.WriteLine($"🎨 Nivel: {level.Name}");
            Console.WriteLine($"   Plataformas: {level.Platforms?.Count ?? 0}");
            Console.WriteLine($"   NPCs: {level.NPCs?.Count ?? 0}");

            // Limpiar el canvas
            _gameCanvas.Children.Clear();
            Console.WriteLine("🗑️ Canvas limpiado");

            // 🔹 RENDERIZAR PLATAFORMAS
            if (level.Platforms != null && level.Platforms.Count > 0)
            {
                Console.WriteLine($"🏗️ Renderizando {level.Platforms.Count} plataformas...");
                
                int platformCount = 0;
                foreach (var platform in level.Platforms)
                {
                    try
                    {
                        var rect = new Rectangle
                        {
                            Fill = new SolidColorBrush(Color.Parse("#0f3460")),
                            Stroke = new SolidColorBrush(Color.Parse("#00ff88")),
                            StrokeThickness = 2,
                            Width = platform.Width,
                            Height = platform.Height
                        };
                        
                        Canvas.SetLeft(rect, platform.X);
                        Canvas.SetTop(rect, platform.Y);
                        _gameCanvas.Children.Add(rect);
                        platformCount++;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"    Error: {ex.Message}");
                    }
                }
                Console.WriteLine($"   ✓ {platformCount} plataformas renderizadas");
            }
            else
            {
                Console.WriteLine("⚠️ No hay plataformas");
            }

            // 🔹 RENDERIZAR NPCs
            if (level.NPCs != null && level.NPCs.Count > 0)
            {
                Console.WriteLine($"👾 Renderizando {level.NPCs.Count} NPCs...");
                
                foreach (var npc in level.NPCs)
                {
                    try
                    {
                        var ellipse = new Ellipse
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
                    catch (Exception ex)
                    {
                        Console.WriteLine($"    Error: {ex.Message}");
                    }
                }
            }

            // 🔹 RENDERIZAR JUGADOR
            if (_viewModel.Player != null)
            {
                try
                {
                    _playerShape = new Rectangle
                    {
                        Fill = new SolidColorBrush(Color.Parse("#e94560")),
                        Stroke = new SolidColorBrush(Color.Parse("#ff0080")),
                        StrokeThickness = 2,
                        Width = 40,
                        Height = 60
                    };
                    
                    Canvas.SetLeft(_playerShape, _viewModel.Player.Position.X - 20);
                    Canvas.SetTop(_playerShape, _viewModel.Player.Position.Y - 60);
                    _gameCanvas.Children.Add(_playerShape);
                    
                    Console.WriteLine($"🎮 Jugador renderizado");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($" Error jugador: {ex.Message}");
                }
            }

            // 🔹 RENDERIZAR PORTAL
            try
            {
                var portal = new Ellipse
                {
                    Fill = new SolidColorBrush(Color.Parse("#00ff88")),
                    Stroke = new SolidColorBrush(Color.Parse("#00ff00")),
                    StrokeThickness = 2,
                    Width = 60,
                    Height = 80,
                    Opacity = 0.6
                };
                
                Canvas.SetLeft(portal, 700 - 30);
                Canvas.SetTop(portal, 420 - 80);
                _gameCanvas.Children.Add(portal);
                
                Console.WriteLine($"🟢 Portal renderizado");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error portal: {ex.Message}");
            }

            Console.WriteLine($" Nivel renderizado. Total elementos en Canvas: {_gameCanvas.Children.Count}");
            Console.WriteLine("🎨 === FIN RENDERIZADO ===\n");
        }

        private void UpdatePlayerPosition()
        {
            if (_gameCanvas == null || _viewModel?.Player == null || _playerShape == null)
                return;

            try
            {
                Canvas.SetLeft(_playerShape, _viewModel.Player.Position.X - 20);
                Canvas.SetTop(_playerShape, _viewModel.Player.Position.Y - 60);
            }
            catch (Exception ex)
            {
                Console.WriteLine($" Error actualizar posición: {ex.Message}");
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