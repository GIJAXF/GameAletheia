using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using GameAletheiaCross.ViewModels;

namespace GameAletheiaCross.Views
{
    public partial class FactionSelectView : UserControl
    {
        public FactionSelectView()
        {
            InitializeComponent();
            
            // Conectar eventos de hover después de InitializeComponent
            this.Loaded += OnLoaded;
        }

        private void OnLoaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            // Obtener referencias a los borders
            var bibliotecaBorder = this.FindControl<Border>("BibliotecaBorder");
            var gobiernoBorder = this.FindControl<Border>("GobiernoBorder");
            var redlineBorder = this.FindControl<Border>("RedlineBorder");
            var neutralBorder = this.FindControl<Border>("NeutralBorder");

            // Conectar eventos PointerEnter (hover)
            if (bibliotecaBorder != null)
                bibliotecaBorder.PointerEntered += OnBibliotecaHover;
            
            if (gobiernoBorder != null)
                gobiernoBorder.PointerEntered += OnGobiernoHover;
            
            if (redlineBorder != null)
                redlineBorder.PointerEntered += OnRedlineHover;
            
            if (neutralBorder != null)
                neutralBorder.PointerEntered += OnNeutralHover;
        }

        private void OnBibliotecaHover(object? sender, PointerEventArgs e)
        {
            if (DataContext is FactionSelectViewModel vm)
            {
                vm.OnBibliotecaHover();
            }
        }

        private void OnGobiernoHover(object? sender, PointerEventArgs e)
        {
            if (DataContext is FactionSelectViewModel vm)
            {
                vm.OnGobiernoHover();
            }
        }

        private void OnRedlineHover(object? sender, PointerEventArgs e)
        {
            if (DataContext is FactionSelectViewModel vm)
            {
                vm.OnRedlineHover();
            }
        }

        private void OnNeutralHover(object? sender, PointerEventArgs e)
        {
            if (DataContext is FactionSelectViewModel vm)
            {
                vm.OnNeutralHover();
            }
        }
    }
}