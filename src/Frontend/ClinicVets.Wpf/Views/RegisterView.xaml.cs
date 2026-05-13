using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicVets.Application.Services;

namespace ClinicVets.Wpf.Views;

public partial class RegisterView : UserControl
{
    private readonly EmployeeRegistrationService _registration;
    private readonly MainWindow _shell;

    public RegisterView(EmployeeRegistrationService registration, MainWindow shell)
    {
        InitializeComponent();
        _registration = registration;
        _shell = shell;
        Loaded += (_, _) =>
        {
            var logo = WpfBranding.LoadLogo();
            if (logo is not null)
                BrandLogo.Source = logo;
        };
    }

    private void OnBack(object sender, RoutedEventArgs e) => _shell.ShowLogin();

    private async void OnSubmit(object sender, RoutedEventArgs e)
    {
        Feedback.Visibility = Visibility.Collapsed;
        var roleItem = RoleCombo.SelectedItem as ComboBoxItem;
        var role = roleItem?.Content?.ToString() ?? "Secretary";
        var result = await _registration.RegisterAsync(FullName.Text, Email.Text, PasswordBox.Password, role);
        if (!result.IsSuccess)
        {
            FeedbackText.Text = result.Message;
            FeedbackText.Foreground = new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));
            Feedback.Background = new SolidColorBrush(Color.FromRgb(0xFE, 0xF2, 0xF2));
            Feedback.Visibility = Visibility.Visible;
            return;
        }

        MessageBox.Show(
            Window.GetWindow(this),
            result.Message + Environment.NewLine + Environment.NewLine +
            "Return to the sign-in screen once an administrator has approved the account.",
            "Success — ClinicVets",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
        _shell.ShowLogin();
    }
}
