using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;

namespace ClinicVets.Wpf.Views;

public partial class LoginView : UserControl
{
    private readonly EmployeeAuthenticationService _auth;
    private readonly MainWindow _shell;

    public LoginView(EmployeeAuthenticationService auth, MainWindow shell)
    {
        InitializeComponent();
        _auth = auth;
        _shell = shell;
        Loaded += (_, _) =>
        {
            var logo = WpfBranding.LoadLogo();
            if (logo is not null)
                BrandLogo.Source = logo;
            DemoButton.Visibility = DesktopBuildOptions.EnableDemoMode ? Visibility.Visible : Visibility.Collapsed;
        };
    }

    private void ShowFeedback(string message, bool isError)
    {
        FeedbackText.Text = message;
        Feedback.Background = new SolidColorBrush(isError ? Color.FromRgb(0xFE, 0xF2, 0xF2) : Color.FromRgb(0xEC, 0xFD, 0xF5));
        FeedbackText.Foreground = new SolidColorBrush(isError ? Color.FromRgb(0xEF, 0x44, 0x44) : Color.FromRgb(0x16, 0x65, 0x34));
        Feedback.Visibility = Visibility.Visible;
    }

    private void ClearFeedback() => Feedback.Visibility = Visibility.Collapsed;

    private async void OnLoginClick(object sender, RoutedEventArgs e)
    {
        ClearFeedback();
        var id = LoginId.Text.Trim();
        var pw = PasswordBox.Password;
        var result = await _auth.LoginAsync(id, pw);
        if (!result.IsSuccess || result.Employee is null)
        {
            ShowFeedback(result.Message, true);
            return;
        }

        _shell.ShowShell(result.Employee, useQuickAccessData: false);
    }

    private void OnRegisterClick(object sender, RoutedEventArgs e) => _shell.ShowRegister();

    private void OnDemoClick(object sender, RoutedEventArgs e) => _shell.EnterDemo();
}
