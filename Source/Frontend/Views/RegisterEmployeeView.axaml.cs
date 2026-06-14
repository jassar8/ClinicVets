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
    public partial class RegisterEmployeeView : UserControl
    {
        public Action? BackToLogin;
        public Action<string>? RegistrationCompleted;
        private bool isPasswordVisible;

        public RegisterEmployeeView()
        {
            InitializeComponent();
        }

        private void ShowMessage(string message)
        {
            UIHelper.ShowMessage(this, message);
        }

        private void ValidateInputs_Changed(object? sender, TextChangedEventArgs e)
        {
            if (ReferenceEquals(sender, EmailInput))
                UpdateEmailValidationText(EmailInput.Text?.Trim() ?? "");

            ValidateInputs(showEmptyMessage: false);
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

        private bool ValidateInputs(bool showEmptyMessage)
        {
            string username = UsernameInput.Text?.Trim() ?? "";
            string password = PasswordInput.Text?.Trim() ?? "";
            string employeeNumber = EmployeeNumberInput.Text?.Trim() ?? "";
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            string email = EmailInput.Text?.Trim() ?? "";

            if (!ValidationService.ValidateEmployeeRegistration(
                    username,
                    password,
                    employeeNumber,
                    idNumber,
                    email,
                    out string? errorMessage,
                    showEmptyFieldsMessage: showEmptyMessage))
            {
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    if (ValidationService.GetEmailValidationMessage(email) != null)
                        UpdateEmailValidationText(email);

                    return ShowValidationError(errorMessage);
                }

                ValidationText.Text = "";
                return false;
            }

            EmailValidationText.Text = "";

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
            isPasswordVisible = !isPasswordVisible;
            PasswordInput.PasswordChar = isPasswordVisible ? '\0' : '*';
            PasswordEyeButton.Content = isPasswordVisible ? UiIcons.HidePassword : UiIcons.ShowPassword;
        }

        private void SaveEmployee_Click(object? sender, RoutedEventArgs e)
        {
            string username = UsernameInput.Text?.Trim() ?? "";
            string password = PasswordInput.Text?.Trim() ?? "";
            string employeeNumber = EmployeeNumberInput.Text?.Trim() ?? "";
            string idNumber = IdNumberInput.Text?.Trim() ?? "";
            string email = EmailInput.Text?.Trim() ?? "";

            if (!ValidateInputs(showEmptyMessage: true))
                return;

            string role = "Secretary";

            if (RoleDropdown.SelectedItem is ComboBoxItem selectedRole &&
                selectedRole.Content != null)
            {
                string roleText = selectedRole.Content.ToString() ?? "מזכיר/ה";
                role = roleText.Contains("וטרינר") ? "Vet" : "Secretary";
            }

            if (!ValidationService.IsValidRole(role))
            {
                ShowMessage("תפקיד לא תקין");
                return;
            }

            bool usernameExists = AppData.Employees.Any(emp => emp.Username == username);

            if (usernameExists)
            {
                ShowMessage("שם המשתמש כבר קיים במערכת");
                return;
            }

            bool idExists = AppData.Employees.Any(emp => emp.IdNumber == idNumber);

            if (idExists)
            {
                ShowMessage("תעודת הזהות כבר קיימת במערכת");
                return;
            }

            bool employeeNumberExists = AppData.Employees.Any(emp => emp.EmployeeNumber == employeeNumber);
            bool emailExists = AppData.Employees.Any(emp => emp.Email == email);

            if (employeeNumberExists || emailExists)
            {
                ShowMessage("מספר עובד או אימייל כבר קיימים במערכת");
                return;
            }

            AppData.Employees.Add(new Employee
            {
                Username = username,
                Password = password,
                EmployeeNumber = employeeNumber,
                IdNumber = idNumber,
                Email = email,
                Role = role
            });

            AppData.SaveEmployeesToDatabase();

            RegistrationCompleted?.Invoke("ההרשמה הושלמה בהצלחה. אפשר להתחבר עם המשתמש החדש.");
        }

        private void Back_Click(object? sender, RoutedEventArgs e)
        {
            BackToLogin?.Invoke();
        }
    }
}