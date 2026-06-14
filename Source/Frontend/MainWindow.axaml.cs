using System;
using Avalonia.Controls;
using Avalonia.Platform;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Views;

namespace ClinicVetsAvalonia
{
    // Root window and navigation host. It tracks the logged-in employee and swaps the
    // current page (login -> register -> main menu -> feature screens) inside MainContent.
    public partial class MainWindow : Window
    {
        private Employee? currentEmployee;

        public MainWindow()
        {
            InitializeComponent();
            SetWindowIcon();

            // Prepare the database and demo data before showing the first screen.
            AppData.Initialize();

            ShowLogin();
        }

        private void SetWindowIcon()
        {
            try
            {
                Icon = new WindowIcon(AssetLoader.Open(new Uri("avares://ClinicVets/Assets/ClinicVets.ico")));
            }
            catch
            {
                // XAML Icon="/Assets/ClinicVets.ico" remains as fallback when asset loader path differs.
            }
        }

        private void ShowLogin()
        {
            ShowLogin("");
        }

        // Shows the login screen. On success we remember the employee and open the menu;
        // the optional status message is shown after actions like a completed registration.
        private void ShowLogin(string statusMessage)
        {
            var loginView = string.IsNullOrWhiteSpace(statusMessage)
                ? new LoginView()
                : new LoginView(statusMessage);

            loginView.LoginSucceeded += employee =>
            {
                currentEmployee = employee;
                ShowMainMenu();
            };

            loginView.RegisterRequested += ShowRegisterEmployee;

            ShowPage(loginView);
        }

        // Shows the employee registration screen and returns to login when done or cancelled.
        private void ShowRegisterEmployee()
        {
            var registerView = new RegisterEmployeeView();

            registerView.BackToLogin += ShowLogin;
            registerView.RegistrationCompleted += ShowLogin;

            ShowPage(registerView);
        }

        // Main menu after login; wires each menu button to its feature screen and logout.
        private void ShowMainMenu()
        {
            if (currentEmployee == null)
            {
                ShowLogin();
                return;
            }

            var mainMenuView = new MainMenuView(currentEmployee);

            mainMenuView.OpenClients += ShowClients;
            mainMenuView.OpenAnimals += ShowAnimals;
            mainMenuView.OpenVisits += ShowVisits;
            mainMenuView.OpenMedicines += ShowMedications;

            mainMenuView.Logout += () =>
            {
                currentEmployee = null;
                ShowLogin();
            };

            ShowPage(mainMenuView);
        }

        private void ShowClients()
        {
            if (!CanOpenScreen("Clients"))
                return;

            var clientsView = new ClientsView();

            clientsView.BackToMainMenu += ShowMainMenu;

            ShowPage(clientsView);
        }

        private void ShowAnimals()
        {
            if (!CanOpenScreen("Animals"))
                return;

            var animalsView = new AnimalsView();

            animalsView.BackToMainMenu += ShowMainMenu;

            ShowPage(animalsView);
        }

        private void ShowVisits()
        {
            if (!CanOpenScreen("Visits"))
                return;

            var visitsView = new VisitsView();

            visitsView.BackToMainMenu += ShowMainMenu;

            ShowPage(visitsView);
        }

        private void ShowMedications()
        {
            if (!CanOpenScreen("Medications"))
                return;

            var medicationsView = new MedicationsView();

            medicationsView.BackToMainMenu += ShowMainMenu;

            ShowPage(medicationsView);
        }

        // Swaps the visible page inside the window's content host.
        private void ShowPage(Control page)
        {
            MainContent.Child = page;
            page.Focus();
        }

        // Role-based access gate. Secretary can open Clients/Animals; Vet can open
        // Animals/Visits/Medications. Anything else is blocked with a message.
        private bool CanOpenScreen(string screenName)
        {
            if (currentEmployee == null)
            {
                ShowLogin();
                return false;
            }

            bool allowed = currentEmployee.Role switch
            {
                "Secretary" => screenName is "Clients" or "Animals",
                "Vet" => screenName is "Animals" or "Visits" or "Medications",
                _ => false
            };

            if (!allowed)
            {
                UIHelper.ShowMessage(this, "אין הרשאה לפתוח מסך זה לפי התפקיד שלך");
                ShowMainMenu();
                return false;
            }

            return true;
        }
    }
}
