using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClinicVets.Desktop.Views.Dashboard;

public partial class ReportsView : UserControl
{
    public Action? BackToMainMenu;
    public ReportsView() => InitializeComponent();
    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();
}
