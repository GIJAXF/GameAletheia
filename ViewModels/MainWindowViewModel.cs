using ReactiveUI;

namespace GameAletheiaCross.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentView;

        public MainWindowViewModel()
        {
            // Inicia con el menú principal y pasa el delegado de navegación
            NavigateTo(new MainMenuViewModel(NavigateTo));
        }

        public ViewModelBase CurrentView
        {
            get => _currentView;
            set => this.RaiseAndSetIfChanged(ref _currentView, value);
        }

        // Acción que los demás ViewModel usarán para cambiar de pantalla
        public void NavigateTo(ViewModelBase viewModel)
        {
            CurrentView = viewModel;
        }
    }
}
