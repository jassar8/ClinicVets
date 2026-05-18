using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClinicVets.Desktop.Views.Dashboard;

public partial class SettingsView : UserControl
{
    public Action? BackToMainMenu;
    public SettingsView() => InitializeComponent();
    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();
}
