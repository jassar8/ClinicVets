using System;
using System.Linq;
using AppKit;
using CoreGraphics;
using ClinicManagementSystem.app.Data;
using ClinicManagementSystem.app.Helpers;
using ClinicManagementSystem.app.Models;

namespace ClinicManagementSystem.app.Views
{
    public class LoginView : NSView
    {
        public Action<Employee> LoginSucceeded;
        public Action RegisterRequested;

        public LoginView() : base(new CGRect(0, 0, 900, 650))
        {
            BuildUI();
        }

        private void BuildUI()
        {
            AddSubview(UIHelper.CreateLabel("Clinic Management System", 220, 540, 460, 50, true));
            AddSubview(UIHelper.CreateLabel("מסך התחברות", 350, 500, 200, 30));

            var usernameInput = UIHelper.CreateInput("Username", 310, 420, 280, 35);
            var passwordInput = UIHelper.CreatePasswordInput("Password", 310, 370, 280, 35);

            AddSubview(usernameInput);
            AddSubview(passwordInput);

            AddSubview(UIHelper.CreateButton("Login", 310, 310, 130, 40, (sender, e) =>
            {
                string username = usernameInput.StringValue.Trim();
                string password = passwordInput.StringValue.Trim();

                var employee = AppData.Employees.FirstOrDefault(emp =>
                    emp.Username == username && emp.Password == password);

                if (employee == null)
                {
                    UIHelper.ShowMessage("שם משתמש או סיסמה שגויים");
                    return;
                }

                LoginSucceeded?.Invoke(employee);
            }));

            AddSubview(UIHelper.CreateButton("Register Employee", 460, 310, 150, 40, (sender, e) =>
            {
                RegisterRequested?.Invoke();
            }));

            AddSubview(UIHelper.CreateLabel("משתמשים לבדיקה:", 310, 240, 280, 25));
            AddSubview(UIHelper.CreateLabel("Secretary: admin / 1234", 310, 215, 280, 25));
            AddSubview(UIHelper.CreateLabel("Vet: vet / 1234", 310, 190, 280, 25));
        }
    }
}

