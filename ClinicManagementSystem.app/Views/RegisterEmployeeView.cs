using System;
using System.Linq;
using AppKit;
using CoreGraphics;
using ClinicManagementSystem.app.Data;
using ClinicManagementSystem.app.Helpers;
using ClinicManagementSystem.app.Models;
using ClinicManagementSystem.app.Services;

namespace ClinicManagementSystem.app.Views
{
    public class RegisterEmployeeView : NSView
    {
        public Action BackToLogin;

        public RegisterEmployeeView() : base(new CGRect(0, 0, 900, 650))
        {
            BuildUI();
        }

        private void BuildUI()
        {
            AddSubview(UIHelper.CreateLabel("Register New Employee", 250, 550, 400, 50, true));

            var usernameInput = UIHelper.CreateInput("Username 6-8 characters", 310, 470, 280, 35);
            var passwordInput = UIHelper.CreatePasswordInput("Password 8-10 chars", 310, 420, 280, 35);
            var employeeNumberInput = UIHelper.CreateInput("Employee Number 4 digits", 310, 370, 280, 35);
            var emailInput = UIHelper.CreateInput("Email", 310, 320, 280, 35);
            var roleDropdown = UIHelper.CreateRoleDropdown(310, 270, 280, 35);

            AddSubview(usernameInput);
            AddSubview(passwordInput);
            AddSubview(employeeNumberInput);
            AddSubview(emailInput);
            AddSubview(roleDropdown);

            AddSubview(UIHelper.CreateButton("Save Employee", 310, 210, 140, 40, (sender, e) =>
            {
                string username = usernameInput.StringValue.Trim();
                string password = passwordInput.StringValue.Trim();
                string employeeNumber = employeeNumberInput.StringValue.Trim();
                string email = emailInput.StringValue.Trim();
                string role = roleDropdown.SelectedItem.Title;

                if (!ValidationService.IsValidUsername(username))
                {
                    UIHelper.ShowMessage("שם משתמש חייב להיות בין 6 ל־8 תווים");
                    return;
                }

                if (!ValidationService.IsValidPassword(password))
                {
                    UIHelper.ShowMessage("סיסמה חייבת להיות 8-10 תווים ולכלול אות, ספרה ותו מיוחד");
                    return;
                }

                if (!ValidationService.IsValidEmployeeNumber(employeeNumber))
                {
                    UIHelper.ShowMessage("מספר עובד חייב להיות 4 ספרות");
                    return;
                }

                if (!ValidationService.IsValidEmail(email))
                {
                    UIHelper.ShowMessage("אימייל לא תקין");
                    return;
                }

                bool usernameExists = AppData.Employees.Any(emp => emp.Username == username);

                if (usernameExists)
                {
                    UIHelper.ShowMessage("שם המשתמש כבר קיים במערכת");
                    return;
                }

                AppData.Employees.Add(new Employee
                {
                    Username = username,
                    Password = password,
                    EmployeeNumber = employeeNumber,
                    Email = email,
                    Role = role
                });

                AppData.SaveEmployeesToDatabase();

                UIHelper.ShowMessage("העובד נשמר בהצלחה");
                BackToLogin?.Invoke();
            }));

            AddSubview(UIHelper.CreateButton("Back", 470, 210, 120, 40, (sender, e) =>
            {
                BackToLogin?.Invoke();
            }));
        }
    }
}