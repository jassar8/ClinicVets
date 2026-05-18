using ClinicVets.Desktop.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVets.Application.Security;
using ClinicVets.Application.Shell;
using ClinicVets.Application.Validation;
using ClinicVets.Core;
using ClinicVets.Desktop.Helpers;

namespace ClinicVets.Desktop.Views.Auth;

public partial class RegisterEmployeeView : UserControl
{
    public Action? BackToLogin;
    public Action<string>? RegistrationCompleted;
    private bool _isPasswordVisible;

    public RegisterEmployeeView() => InitializeComponent();

    private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e) =>
        ValidateInputs(showEmptyMessage: false);

    private bool ValidateInputs(bool showEmptyMessage)
    {
        if (string.IsNullOrWhiteSpace(UsernameInput.Text) ||
            string.IsNullOrWhiteSpace(PasswordInput.Text) ||
            string.IsNullOrWhiteSpace(EmailInput.Text))
        {
            ValidationText.Foreground = Avalonia.Media.Brushes.Firebrick;
            ValidationText.Text = showEmptyMessage ? "?? ???? ??, ?????? ??????" : "";
            return false;
        }

        if (!EmployeeInputValidation.IsValidUsername(UsernameInput.Text!))
            return ShowValidationError("?? ?????: 6-8 ????? ??????? (?????? ?? ?????)");

        if (!UiFormValidation.IsValidPassword(PasswordInput.Text!))
            return ShowValidationError("????? ????? ????? 8-10 ????? ?????? ???, ???? ??? ?????");

        if (!UiFormValidation.IsValidEmail(EmailInput.Text!))
            return ShowValidationError("?????? ?? ????");

        ValidationText.Foreground = Avalonia.Media.Brushes.ForestGreen;
        ValidationText.Text = "?????? ????? ??????";
        return true;
    }

    private bool ShowValidationError(string message)
    {
        ValidationText.Foreground = Avalonia.Media.Brushes.Firebrick;
        ValidationText.Text = message;
        return false;
    }

    private void TogglePasswordVisibility(object? sender, RoutedEventArgs e)
    {
        _isPasswordVisible = !_isPasswordVisible;
        PasswordInput.PasswordChar = _isPasswordVisible ? '\0' : '*';
        PasswordEyeButton.Content = _isPasswordVisible ? "??" : "??";
    }

    private async void SaveEmployee_Click(object? sender, RoutedEventArgs e)
    {
        if (AppServices.IsDemoMode)
        {
            UIHelper.ShowMessage(this, "?? ???? ????? ??????? ???? ???. ?? ???? ??? ?????? ????? ?????.");
            return;
        }

        if (!ValidateInputs(showEmptyMessage: true))
            return;

        var role = EmployeeRole.Secretary;
        if (RoleDropdown.SelectedItem is ComboBoxItem selected && selected.Content is not null)
        {
            var roleText = selected.Content.ToString() ?? "?????/?";
            role = roleText.Contains("??????") ? EmployeeRole.Veterinarian : EmployeeRole.Secretary;
        }

        var username = UsernameInput.Text!.Trim();
        var result = await AppServices.Registration.RegisterAsync(
            fullName: username,
            email: EmailInput.Text!.Trim(),
            password: PasswordInput.Text!.Trim(),
            role: EmployeeRoleNames.ToStoredString(role),
            username: username);

        if (!result.IsSuccess)
        {
            UIHelper.ShowMessage(this, result.Message);
            return;
        }

        var successHebrew = DesktopBuildOptions.AutoApproveSelfRegistration
            ? "?????? ???? ??????. ???? ?????? ?? ?? ?????? ?? ???????."
            : "?????? ??????. ?????? ????? ?????? ???? ???? ???????.";
        UIHelper.ShowMessage(this, successHebrew);
        RegistrationCompleted?.Invoke(successHebrew);
    }

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToLogin?.Invoke();
}
