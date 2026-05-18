using ClinicVets.Desktop.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVets.Desktop.Helpers;

namespace ClinicVets.Desktop.Views.Auth;

public partial class ForgotPasswordView : UserControl
{
    public Action? BackToLogin;
    public Action<string>? PasswordResetCompleted;
    private bool _isNewPasswordVisible;
    private bool _isConfirmPasswordVisible;

    public ForgotPasswordView() => InitializeComponent();

    private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
    {
        StatusText.Text = "";
        ValidateCurrentFields(showEmptyMessage: false);
    }

    private async void SendCode_Click(object? sender, RoutedEventArgs e)
    {
        var email = EmailInput.Text?.Trim() ?? "";
        if (!UiFormValidation.IsValidEmail(email))
        {
            ShowValidationError("?? ????? ?????? ????");
            return;
        }

        try
        {
            var result = await AppServices.PasswordReset.RequestCodeAsync(email);
            if (!result.IsSuccess)
            {
                ShowValidationError(result.Message);
                return;
            }

            ShowStatus(result.Message);
        }
        catch (Exception ex)
        {
            ShowValidationError($"????? ??????? ?????: {ex.Message}");
        }
    }

    private async void ResetPassword_Click(object? sender, RoutedEventArgs e)
    {
        if (!ValidateCurrentFields(showEmptyMessage: true))
            return;

        var result = await AppServices.PasswordReset.ResetPasswordAsync(
            EmailInput.Text ?? "",
            CodeInput.Text ?? "",
            NewPasswordInput.Text ?? "");

        if (!result.IsSuccess)
        {
            ShowValidationError(result.Message);
            return;
        }

        PasswordResetCompleted?.Invoke("?????? ?????? ??????. ???? ?????? ?? ?????? ?????.");
    }

    private bool ValidateCurrentFields(bool showEmptyMessage)
    {
        var email = EmailInput.Text?.Trim() ?? "";
        var code = CodeInput.Text?.Trim() ?? "";
        var newPassword = NewPasswordInput.Text?.Trim() ?? "";
        var confirmPassword = ConfirmPasswordInput.Text?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(code) &&
            string.IsNullOrWhiteSpace(newPassword) && string.IsNullOrWhiteSpace(confirmPassword))
        {
            ValidationText.Text = showEmptyMessage ? "?? ???? ??????, ??? ?????? ????" : "";
            return false;
        }

        if (!string.IsNullOrWhiteSpace(email) && !UiFormValidation.IsValidEmail(email))
            return ShowValidationError("?????? ?? ????");

        if (!string.IsNullOrWhiteSpace(code) && (code.Length != 6 || !code.All(char.IsDigit)))
            return ShowValidationError("??? ????? ???? ????? 6 ?????");

        if (!string.IsNullOrWhiteSpace(newPassword) && !UiFormValidation.IsValidPassword(newPassword))
            return ShowValidationError("????? ???? ????? ????? 8-10 ????? ?????? ???, ???? ??? ?????");

        if (!string.IsNullOrWhiteSpace(confirmPassword) && newPassword != confirmPassword)
            return ShowValidationError("????? ?????? ?? ???? ?????? ?????");

        if (showEmptyMessage && (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code) ||
                                 string.IsNullOrWhiteSpace(newPassword) || string.IsNullOrWhiteSpace(confirmPassword)))
            return ShowValidationError("?? ???? ?? ?? ?????");

        ValidationText.Foreground = Avalonia.Media.Brushes.ForestGreen;
        ValidationText.Text = string.IsNullOrWhiteSpace(email) ? "" : "?????? ????? ??????";
        return true;
    }

    private bool ShowValidationError(string message)
    {
        ValidationText.Foreground = Avalonia.Media.Brushes.Firebrick;
        ValidationText.Text = message;
        return false;
    }

    private void ShowStatus(string message)
    {
        StatusText.Text = message;
        ValidationText.Text = "";
    }

    private void ToggleNewPasswordVisibility(object? sender, RoutedEventArgs e)
    {
        _isNewPasswordVisible = !_isNewPasswordVisible;
        NewPasswordInput.PasswordChar = _isNewPasswordVisible ? '\0' : '*';
        NewPasswordEyeButton.Content = _isNewPasswordVisible ? "??" : "??";
    }

    private void ToggleConfirmPasswordVisibility(object? sender, RoutedEventArgs e)
    {
        _isConfirmPasswordVisible = !_isConfirmPasswordVisible;
        ConfirmPasswordInput.PasswordChar = _isConfirmPasswordVisible ? '\0' : '*';
        ConfirmPasswordEyeButton.Content = _isConfirmPasswordVisible ? "??" : "??";
    }

    private void Back_Click(object? sender, RoutedEventArgs e) => BackToLogin?.Invoke();
}
