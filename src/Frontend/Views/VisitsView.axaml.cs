using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClinicVets.Desktop.Views;

public partial class VisitsView : UserControl
{
    public Action? BackToMainMenu;

    public VisitsView()
    {
        InitializeComponent();
    }

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();
}
