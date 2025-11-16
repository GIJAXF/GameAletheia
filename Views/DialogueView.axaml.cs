using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using GameAletheiaCross.ViewModels;
using System.Reactive;

namespace GameAletheiaCross.Views;

public partial class DialogueView : UserControl
{
    public DialogueView()
    {
        InitializeComponent();
        
        // Hacer que el control pueda recibir focus
        Focusable = true;
        
        // Capturar tecla E
        KeyDown += OnKeyDown;
        
        // Solicitar focus cuando se carga
        Loaded += (s, e) => Focus();
    }
    
    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.E && DataContext is DialogueViewModel vm)
        {
            vm.ContinueCommand.Execute(Unit.Default);
            e.Handled = true;
        }
    }
}