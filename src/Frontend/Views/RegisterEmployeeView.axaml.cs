using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVets.Application.Security;
using ClinicVets.Core;
using ClinicVets.Desktop.Helpers;

namespace ClinicVets.Desktop.Views;

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
            ValidationText.Text = showEmptyMessage ? "יש למלא שם, אימייל וסיסמה" : "";
            return false;
        }

        if (!UiFormValidation.IsValidPassword(PasswordInput.Text!))
            return ShowValidationError("סיסמה חייבת להיות 8-10 תווים ולכלול אות, ספרה ותו מיוחד");

        if (!UiFormValidation.IsValidEmail(EmailInput.Text!))
            return ShowValidationError("אימייל לא תקין");

        ValidationText.Foreground = Avalonia.Media.Brushes.ForestGreen;
        ValidationText.Text = "הפרטים נראים תקינים";
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
        PasswordEyeButton.Content = _isPasswordVisible ? "🙈" : "👁";
    }

    private async void SaveEmployee_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidateInputs(showEmptyMessage: true))
            return;

        var role = EmployeeRole.Secretary;
        if (RoleDropdown.SelectedItem is ComboBoxItem selected && selected.Content is not null)
        {
            var roleText = selected.Content.ToString() ?? "מזכיר/ה";
            role = roleText.Contains("וטרינר") ? EmployeeRole.Veterinarian : EmployeeRole.Secretary;
        }

        var result = await AppServices.Registration.RegisterAsync(
            UsernameInput.Text!.Trim(),
            EmailInput.Text!.Trim(),
            PasswordInput.Text!.Trim(),
            EmployeeRoleNames.ToStoredString(role));

        if (!result.IsSuccess)
        {
            UIHelper.ShowMessage(this, result.Message);
            return;
        }

        RegistrationCompleted?.Invoke("ההרשמה הושלמה. חשבונך ממתין לאישור מנהל לפני התחברות.");
    }

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToLogin?.Invoke();
}
