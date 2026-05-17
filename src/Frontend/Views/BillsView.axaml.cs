using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClinicVets.Desktop.Views;

public partial class BillsView : UserControl
{
    public Action? BackToMainMenu;

    public BillsView() => InitializeComponent();

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();
}
