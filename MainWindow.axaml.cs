using Avalonia.Controls;
using ClinicVetsAvalonia.Repositories;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.ViewModels;
using ClinicVetsAvalonia.Views.Animals;
using ClinicVetsAvalonia.Views.Auth;
using ClinicVetsAvalonia.Views.Clients;
using ClinicVetsAvalonia.Views.Dashboard;
using ClinicVetsAvalonia.Views.Employees;
using ClinicVetsAvalonia.Views.Medicine;
using ClinicVetsAvalonia.Views.Visits;

namespace ClinicVetsAvalonia;

public partial class MainWindow : Window
{
    private readonly AppSession _session = new();

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
            _session.CurrentEmployee = employee;
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
        if (_session.CurrentEmployee == null)
        {
            ShowLogin();
            return;
        }

        var mainMenuView = new MainMenuView(_session.CurrentEmployee);

        mainMenuView.OpenClients += ShowClients;
        mainMenuView.OpenAnimals += ShowAnimals;
        mainMenuView.OpenVisits += ShowVisits;
        mainMenuView.OpenMedicines += ShowMedications;

        mainMenuView.Logout += () =>
        {
            _session.CurrentEmployee = null;
            ShowLogin();
        };

        ShowPage(mainMenuView);
    }

    private void ShowClients()
    {
        var clientsView = new ClientsView();

        clientsView.BackToMainMenu += ShowMainMenu;

        ShowPage(clientsView);
    }

    private void ShowAnimals()
    {
        var animalsView = new AnimalsView();

        animalsView.BackToMainMenu += ShowMainMenu;

        ShowPage(animalsView);
    }

    private void ShowVisits()
    {
        var visitsView = new VisitsView();

        visitsView.BackToMainMenu += ShowMainMenu;

        ShowPage(visitsView);
    }

    private void ShowMedications()
    {
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
}
