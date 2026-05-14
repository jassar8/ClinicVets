using Avalonia.Controls;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Helpers;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Views;

namespace ClinicVetsAvalonia
{
    public partial class MainWindow : Window
    {
        private Employee? currentEmployee;

        public MainWindow()
        {
            InitializeComponent();

            AppData.Initialize();

            ShowLogin();
        }

        private void ShowLogin()
        {
            ShowLogin("");
        }

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
            loginView.ForgotPasswordRequested += ShowForgotPassword;

            ShowPage(loginView);
        }

        private void ShowRegisterEmployee()
        {
            var registerView = new RegisterEmployeeView();

            registerView.BackToLogin += ShowLogin;
            registerView.RegistrationCompleted += ShowLogin;

            ShowPage(registerView);
        }

        private void ShowForgotPassword()
        {
            var forgotPasswordView = new ForgotPasswordView();

            forgotPasswordView.BackToLogin += ShowLogin;
            forgotPasswordView.PasswordResetCompleted += ShowLogin;

            ShowPage(forgotPasswordView);
        }

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

        private void ShowPage(Control page)
        {
            page.Opacity = 0.98;
            MainContent.Child = page;
            page.Focus();
        }

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
