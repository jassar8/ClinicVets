using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Views
{
    public partial class LoginView : UserControl
    {
        public Action<Employee>? LoginSucceeded;
        public Action? RegisterRequested;
        private bool isPasswordVisible;

        public LoginView()
        {
            InitializeComponent();
        }

        public LoginView(string statusMessage) : this()
        {
            StatusText.Text = statusMessage;
        }

        private void ShowMessage(string message)
        {
            UIHelper.ShowMessage(this, message);
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            StatusText.Text = "";
            ValidateInputs(showEmptyMessage: false);
        }

        private bool ValidateInputs(bool showEmptyMessage)
        {
            string username = UsernameInput.Text?.Trim() ?? "";
            string password = PasswordInput.Text?.Trim() ?? "";

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            {
                ValidationText.Foreground = Avalonia.Media.Brushes.Firebrick;
                ValidationText.Text = showEmptyMessage ? "יש למלא שם משתמש וסיסמה" : "";
                return false;
            }

            if (!ValidationService.IsValidUsername(username))
            {
                ValidationText.Text = "שם משתמש צריך להיות 6-8 תווים באנגלית, עד 2 ספרות";
                return false;
            }

            ValidationText.Text = "";
            return true;
        }

        private void TogglePasswordVisibility(object? sender, RoutedEventArgs e)
        {
            isPasswordVisible = !isPasswordVisible;
            PasswordInput.PasswordChar = isPasswordVisible ? '\0' : '*';
            PasswordEyeButton.Content = isPasswordVisible ? UiIcons.HidePassword : UiIcons.ShowPassword;
        }

        // Delegates the actual login decision to AuthService and reacts to the result:
        // show the error on failure, or raise LoginSucceeded so MainWindow opens the menu.
        private void Login_Click(object? sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text?.Trim() ?? "";
            string password = PasswordInput.Text?.Trim() ?? "";

            var result = AuthService.TryLogin(username, password);

            if (!result.Success)
            {
                ValidationText.Foreground = Avalonia.Media.Brushes.Firebrick;
                ValidationText.Text = result.ErrorMessage;

                if (result.Reason == LoginFailureReason.InvalidCredentials)
                    ShowMessage(result.ErrorMessage);

                return;
            }

            LoginSucceeded?.Invoke(result.Employee!);
        }

        private void Register_Click(object? sender, RoutedEventArgs e)
        {
            RegisterRequested?.Invoke();
        }
    }
}