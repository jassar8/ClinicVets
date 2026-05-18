using ClinicVets.Desktop.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVets.Application.Shell;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers;

namespace ClinicVets.Desktop.Views.Auth;

public partial class LoginView : UserControl
{
    public Action<Employee>? LoginSucceeded;
    public Action? RegisterRequested;
    public Action? ForgotPasswordRequested;
    public Action<Employee, EmployeeRole>? DemoModeRequested;
    private bool _isPasswordVisible;

    public LoginView()
    {
        InitializeComponent();
        ApplyDemoModeVisibility();
    }

    public LoginView(string statusMessage) : this() => StatusText.Text = statusMessage;

    private void ApplyDemoModeVisibility()
    {
        var enabled = DesktopBuildOptions.EnableDemoMode;
        DemoRoleLabel.IsVisible = enabled;
        DemoRoleSelectorShell.IsVisible = enabled;
        DemoModeButton.IsVisible = enabled;
        DemoHintText.IsVisible = enabled;
    }

    internal static EmployeeRole GetDemoRoleFromSelectorIndex(int selectedIndex) =>
        selectedIndex switch
        {
            1 => EmployeeRole.Veterinarian,
            2 => EmployeeRole.Secretary,
            _ => EmployeeRole.Admin
        };

    private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
    {
        StatusText.Text = "";
        ValidateInputs(showEmptyMessage: false);
    }

    private bool ValidateInputs(bool showEmptyMessage)
    {
        var login = UsernameInput.Text?.Trim() ?? "";
        var password = PasswordInput.Text?.Trim() ?? "";
        if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
        {
            ValidationText.Foreground = Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = showEmptyMessage ? "?? ???? ?? ????? ??????" : "";
            return false;
        }

        ValidationText.Text = "";
        return true;
    }

    private void TogglePasswordVisibility(object? sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordInput.PasswordChar = _isPasswordVisible ? '\0' : '*';
        PasswordEyeButton.Content = _isPasswordVisible ? "??" : "??";
    }

    private async void Login_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidateInputs(showEmptyMessage: true))
            return;

        var result = await AppServices.Auth.LoginAsync(UsernameInput.Text ?? "", PasswordInput.Text ?? "");
        if (!result.IsSuccess || result.Employee is null)
        {
            ValidationText.Text = "?? ????? ?? ????? ??????";
            UIHelper.ShowMessage(this, result.Message);
            return;
        }

        LoginSucceeded?.Invoke(result.Employee);
    }

    private void Register_Click(object? sender, RoutedEventArgs e) => RegisterRequested?.Invoke();

    private void ForgotPassword_Click(object? sender, RoutedEventArgs e) => ForgotPasswordRequested?.Invoke();

    private void DemoMode_Click(object? sender, RoutedEventArgs e)
    {
        if (!DesktopBuildOptions.EnableDemoMode)
            return;

        if (!AppServices.TryEnterDemoMode(out var demoAdmin, out var errorMessage))
        {
            UIHelper.ShowMessage(this, errorMessage);
            return;
        }

        var demoRole = GetDemoRoleFromSelectorIndex(DemoRoleSelector.SelectedIndex);
        DemoModeRequested?.Invoke(demoAdmin, demoRole);
    }
}
