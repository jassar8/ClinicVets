using System;
using AppKit;
using CoreGraphics;
using ClinicManagementSystem.app.Helpers;
using ClinicManagementSystem.app.Models;

namespace ClinicManagementSystem.app.Views
{
    public class MainMenuView : NSView
    {
        private readonly Employee currentEmployee;

        public Action OpenClients;
        public Action OpenAnimals;
        public Action OpenVisits;
        public Action OpenMedicines;
        public Action Logout;

        public MainMenuView(Employee employee) : base(new CGRect(0, 0, 900, 650))
        {
            currentEmployee = employee;
            BuildUI();
        }

        private void BuildUI()
        {
            AddSubview(UIHelper.CreateLabel("Main Menu", 300, 560, 300, 50, true));

            AddSubview(UIHelper.CreateLabel(
                $"Logged in as: {currentEmployee.Username} | Role: {currentEmployee.Role}",
                230,
                520,
                440,
                30
            ));

            var clientsButton = UIHelper.CreateButton("ניהול לקוחות", 330, 440, 240, 45, (sender, e) =>
            {
                OpenClients?.Invoke();
            });

            var animalsButton = UIHelper.CreateButton("ניהול בעלי חיים", 330, 380, 240, 45, (sender, e) =>
            {
                OpenAnimals?.Invoke();
            });

            var visitsButton = UIHelper.CreateButton("ניהול ביקורים וטיפולים", 330, 320, 240, 45, (sender, e) =>
            {
                OpenVisits?.Invoke();
            });

            var medicinesButton = UIHelper.CreateButton("ניהול תרופות", 330, 260, 240, 45, (sender, e) =>
            {
                OpenMedicines?.Invoke();
            });

            var logoutButton = UIHelper.CreateButton("Logout", 330, 180, 240, 45, (sender, e) =>
            {
                Logout?.Invoke();
            });

            if (currentEmployee.Role == "Secretary")
            {
                clientsButton.Enabled = true;
                animalsButton.Enabled = true;
                visitsButton.Enabled = false;
                medicinesButton.Enabled = false;
            }
            else if (currentEmployee.Role == "Vet")
            {
                clientsButton.Enabled = false;
                animalsButton.Enabled = true;
                visitsButton.Enabled = true;
                medicinesButton.Enabled = true;
            }

            AddSubview(clientsButton);
            AddSubview(animalsButton);
            AddSubview(visitsButton);
            AddSubview(medicinesButton);
            AddSubview(logoutButton);
        }
    }
}