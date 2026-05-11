using System;
using AppKit;
using CoreGraphics;
using ClinicManagementSystem.app.Models;
using ClinicManagementSystem.app.Views;
using ClinicManagementSystem.app.Models;
using ClinicManagementSystem.app.Views;
using ClinicManagementSystem.app.Data;

namespace ClinicManagementSystem.app
{
    public partial class ViewController : NSViewController
    {
        private Employee currentEmployee;

        public ViewController(IntPtr handle) : base(handle)
        {
        }

        public override void LoadView()
        {
            View = new NSView(new CGRect(0, 0, 900, 650));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            AppData.Initialize();

            ShowLoginPage();
        }

        private void SetPage(NSView page)
        {
            foreach (var subview in View.Subviews)
            {
                subview.RemoveFromSuperview();
            }

            View.AddSubview(page);
        }

        private void ShowLoginPage()
        {
            var loginView = new LoginView();

            loginView.LoginSucceeded = (employee) =>
            {
                currentEmployee = employee;
                ShowMainMenuPage();
            };

            loginView.RegisterRequested = () =>
            {
                ShowRegisterEmployeePage();
            };

            SetPage(loginView);
        }

        private void ShowRegisterEmployeePage()
        {
            var registerView = new RegisterEmployeeView();

            registerView.BackToLogin = () =>
            {
                ShowLoginPage();
            };

            SetPage(registerView);
        }

        private void ShowMainMenuPage()
        {
            var mainMenuView = new MainMenuView(currentEmployee);

            mainMenuView.OpenClients = () =>
            {
                ShowClientsPage();
            };

            mainMenuView.OpenAnimals = () =>
            {
                ShowEmptyPage(
                    "ניהול בעלי חיים",
                    "דף ריק - יפותח על ידי צוות 2"
                );
            };

            mainMenuView.OpenVisits = () =>
            {
                ShowEmptyPage(
                    "ניהול ביקורים וטיפולים",
                    "דף ריק - יפותח על ידי צוות 3"
                );
            };

            mainMenuView.OpenMedicines = () =>
            {
                ShowEmptyPage(
                    "ניהול תרופות",
                    "דף ריק - יפותח על ידי צוות 3"
                );
            };

            mainMenuView.Logout = () =>
            {
                currentEmployee = null;
                ShowLoginPage();
            };

            SetPage(mainMenuView);
        }

        private void ShowClientsPage()
        {
            var clientsView = new ClientsView();

            clientsView.BackToMainMenu = () =>
            {
                ShowMainMenuPage();
            };

            clientsView.OpenClientAnimalsPage = () =>
            {
                ShowEmptyPage(
                    "בעלי חיים של לקוח",
                    "דף ריק - יפותח על ידי צוות 2"
                );
            };

            SetPage(clientsView);
        }

        private void ShowEmptyPage(string title, string message)
        {
            var emptyPageView = new EmptyPageView(title, message);

            emptyPageView.BackToMainMenu = () =>
            {
                ShowMainMenuPage();
            };

            SetPage(emptyPageView);
        }
    }
}