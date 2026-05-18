using Avalonia.Controls;
using Avalonia.Interactivity;

namespace ClinicVets.Desktop.Views.Shared;

public partial class EmptyPageView : UserControl
{
    public Action? BackToMainMenu;

    public EmptyPageView() => InitializeComponent();

    public EmptyPageView(string title, string message) : this()
    {
        TitleText.Text = title;
        MessageText.Text = message;
    }

    private void BackButton_Click(object? sender, RoutedEventArgs e) => BackToMainMenu?.Invoke();
}
