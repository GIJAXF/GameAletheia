using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GameAletheiaCross.ViewModels;

namespace GameAletheiaCross.Views
{
    public partial class GameView : UserControl
    {
        public GameView()
        {
            InitializeComponent();
            
            // 🔥 Asegura que el control reciba el foco para capturar el teclado
            Focusable = true;
            AttachedToVisualTree += (_, _) =>
            {
                Focus();
                AddHandler(KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
                AddHandler(KeyUpEvent, OnKeyUp, RoutingStrategies.Tunnel);
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
