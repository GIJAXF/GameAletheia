using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using GameAletheiaCross.Models;
using GameAletheiaCross.ViewModels;

namespace GameAletheiaCross.Views
{
    public partial class GameView : UserControl
    {
        private Canvas? _gameCanvas;
        private GameViewModel? _viewModel;

        public GameView()
        {
            InitializeComponent();
            
            // 🔥 Asegura que el control reciba el foco para capturar el teclado
            Focusable = true;
            AttachedToVisualTree += (_, _) =>
            {
                Focus();
                _gameCanvas = this.FindControl<Canvas>("GameCanvas");
                _viewModel = (GameViewModel)DataContext;
                
                if (_viewModel != null)
                {
                    // Renderizar plataformas cuando cambie el nivel
                    _viewModel.PropertyChanged += (s, e) =>
                    {
                        if (e.PropertyName == nameof(GameViewModel.CurrentLevel))
                        {
                            RenderLevel();
                        }
                        else if (e.PropertyName == nameof(GameViewModel.Player))
                        {
                            UpdatePlayerPosition();
                        }
                    };
                }
                
                AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
                AddHandler(KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
            };
        }

        private void RenderLevel()
        {
            if (_gameCanvas == null || _viewModel?.CurrentLevel == null)
                return;

            // Limpiar el canvas (mantener solo el Grid de UI que está encima)
            _gameCanvas.Children.Clear();

            var level = _viewModel.CurrentLevel;

            // Renderizar plataformas
            if (level.Platforms != null)
            {
                foreach (var platform in level.Platforms)
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
                }
            }

            // Renderizar NPCs
            if (level.NPCs != null)
            {
                foreach (var npc in level.NPCs)
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
            }

            // Renderizar jugador
            if (_viewModel.Player != null)
            {
                var playerRect = new Rectangle
                {
                    Fill = new SolidColorBrush(Color.Parse("#e94560")),
                    Stroke = new SolidColorBrush(Color.Parse("#ff0080")),
                    StrokeThickness = 2,
                    Width = 40,
                    Height = 60
                };
                Canvas.SetLeft(playerRect, _viewModel.Player.Position.X - 20);
                Canvas.SetTop(playerRect, _viewModel.Player.Position.Y - 60);
                _gameCanvas.Children.Add(playerRect);
            }

            // Renderizar portal (salida)
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
        }

        private void UpdatePlayerPosition()
        {
            if (_gameCanvas == null || _viewModel?.Player == null)
                return;

            // Encontrar y actualizar el rectángulo del jugador
            // Es el penúltimo elemento (antes del portal)
            if (_gameCanvas.Children.Count >= 2)
            {
                var playerRect = _gameCanvas.Children[_gameCanvas.Children.Count - 2] as Rectangle;
                if (playerRect != null && playerRect.Fill is SolidColorBrush brush && 
                    brush.Color == Color.Parse("#e94560"))
                {
                    Canvas.SetLeft(playerRect, _viewModel.Player.Position.X - 20);
                    Canvas.SetTop(playerRect, _viewModel.Player.Position.Y - 60);
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