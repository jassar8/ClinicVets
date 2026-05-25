using System;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views
{
    public partial class ForgotPasswordView : UserControl
    {
        private Employee? resetEmployee;
        private string currentCode = "";
        private DateTime codeExpiresAt;
        private bool isNewPasswordVisible;
        private bool isConfirmPasswordVisible;

        public Action? BackToLogin;
        public Action<string>? PasswordResetCompleted;

        public ForgotPasswordView()
        {
            InitializeComponent();
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            StatusText.Text = "";

            if (ReferenceEquals(sender, EmailInput))
                UpdateEmailValidationText(EmailInput.Text?.Trim() ?? "");

            ValidateCurrentFields(showEmptyMessage: false);
        }

        private void UpdateEmailValidationText(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                EmailValidationText.Text = "";
                return;
            }

            EmailValidationText.Text = ValidationService.GetEmailValidationMessage(email) ?? "";
        }

        private async void SendCode_Click(object? sender, RoutedEventArgs e)
        {
            string email = EmailInput.Text?.Trim() ?? "";

            if (!ValidationService.IsValidEmail(email))
            {
                UpdateEmailValidationText(email);
                ShowValidationError(ValidationService.GetEmailValidationMessage(email) ?? "אימייל לא תקין");
                return;
            }

            EmailValidationText.Text = "";

            resetEmployee = AppData.Employees.FirstOrDefault(emp =>
                string.Equals(emp.Email, email, StringComparison.OrdinalIgnoreCase));

            if (resetEmployee == null)
            {
                ShowValidationError("לא נמצא עובד עם האימייל הזה");
                return;
            }

            currentCode = PasswordResetService.GenerateCode();
            codeExpiresAt = DateTime.Now.AddMinutes(10);

            try
            {
                string resultMessage = await PasswordResetService.SendResetCodeAsync(email, currentCode);
                ShowStatus(resultMessage);
            }
            catch (Exception ex)
            {
                ShowValidationError($"שליחת האימייל נכשלה: {ex.Message}");
            }
        }

        private void ResetPassword_Click(object? sender, RoutedEventArgs e)
        {
            if (!ValidateCurrentFields(showEmptyMessage: true))
                return;

            if (resetEmployee == null || string.IsNullOrWhiteSpace(currentCode))
            {
                ShowValidationError("קודם צריך לשלוח קוד אימות לאימייל");
                return;
            }

            if (DateTime.Now > codeExpiresAt)
            {
                ShowValidationError("הקוד פג תוקף. יש לשלוח קוד חדש");
                return;
            }

            string code = CodeInput.Text?.Trim() ?? "";

            if (code != currentCode)
            {
                ShowValidationError("קוד האימות שגוי");
                return;
            }

            resetEmployee.Password = NewPasswordInput.Text?.Trim() ?? "";
            AppData.SaveEmployeesToDatabase();

            PasswordResetCompleted?.Invoke("הסיסמה עודכנה בהצלחה. אפשר להתחבר עם הסיסמה החדשה.");
        }

        private bool ValidateCurrentFields(bool showEmptyMessage)
        {
            string email = EmailInput.Text?.Trim() ?? "";
            string code = CodeInput.Text?.Trim() ?? "";
            string newPassword = NewPasswordInput.Text?.Trim() ?? "";
            string confirmPassword = ConfirmPasswordInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(email) &&
                string.IsNullOrWhiteSpace(code) &&
                string.IsNullOrWhiteSpace(newPassword) &&
                string.IsNullOrWhiteSpace(confirmPassword))
            {
                ValidationText.Text = showEmptyMessage ? "יש למלא אימייל, קוד וסיסמה חדשה" : "";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(email) && !ValidationService.IsValidEmail(email))
            {
                UpdateEmailValidationText(email);
                return ShowValidationError(ValidationService.GetEmailValidationMessage(email) ?? "אימייל לא תקין");
            }

            if (!string.IsNullOrWhiteSpace(email))
                EmailValidationText.Text = "";

            if (!string.IsNullOrWhiteSpace(code) && (code.Length != 6 || !code.All(char.IsDigit)))
                return ShowValidationError("קוד אימות חייב להכיל 6 ספרות");

            if (!string.IsNullOrWhiteSpace(newPassword) && !ValidationService.IsValidPassword(newPassword))
                return ShowValidationError("סיסמה חדשה חייבת להיות 8-10 תווים ולכלול אות, ספרה ותו מיוחד");

            if (!string.IsNullOrWhiteSpace(confirmPassword) && newPassword != confirmPassword)
                return ShowValidationError("אימות הסיסמה לא תואם לסיסמה החדשה");

            if (showEmptyMessage &&
                (string.IsNullOrWhiteSpace(email) ||
                 string.IsNullOrWhiteSpace(code) ||
                 string.IsNullOrWhiteSpace(newPassword) ||
                 string.IsNullOrWhiteSpace(confirmPassword)))
            {
                return ShowValidationError("יש למלא את כל השדות");
            }

            ValidationText.Foreground = Avalonia.Media.Brushes.ForestGreen;
            ValidationText.Text = string.IsNullOrWhiteSpace(email) ? "" : "הפרטים נראים תקינים";
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
            isNewPasswordVisible = !isNewPasswordVisible;
            NewPasswordInput.PasswordChar = isNewPasswordVisible ? '\0' : '*';
            NewPasswordEyeButton.Content = isNewPasswordVisible ? UiIcons.HidePassword : UiIcons.ShowPassword;
        }

        private void ToggleConfirmPasswordVisibility(object? sender, RoutedEventArgs e)
        {
            isConfirmPasswordVisible = !isConfirmPasswordVisible;
            ConfirmPasswordInput.PasswordChar = isConfirmPasswordVisible ? '\0' : '*';
            ConfirmPasswordEyeButton.Content = isConfirmPasswordVisible ? UiIcons.HidePassword : UiIcons.ShowPassword;
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToLogin?.Invoke();
        }
    }
}
