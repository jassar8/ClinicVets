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
    public partial class LoginView : UserControl
    {
        public Action<Employee>? LoginSucceeded;
        public Action? RegisterRequested;
        public Action? ForgotPasswordRequested;
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

        private void Login_Click(object? sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text?.Trim() ?? "";
            string password = PasswordInput.Text?.Trim() ?? "";

            if (!ValidateInputs(showEmptyMessage: true))
                return;

            var employee = AppData.Employees.FirstOrDefault(emp =>
                emp.Username == username && emp.Password == password);

            if (employee == null)
            {
                ValidationText.Text = "שם משתמש או סיסמה שגויים";
                ShowMessage("שם משתמש או סיסמה שגויים");
                return;
            }

            LoginSucceeded?.Invoke(employee);
        }

        private void Register_Click(object? sender, RoutedEventArgs e)
        {
            RegisterRequested?.Invoke();
        }

        private void ForgotPassword_Click(object? sender, RoutedEventArgs e)
        {
            ForgotPasswordRequested?.Invoke();
        }
    }
}