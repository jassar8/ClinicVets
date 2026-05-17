using Avalonia.Controls;
using ClinicVets.Application.Security;
using ClinicVets.Application.Shell;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers;
using ClinicVets.Desktop.Views;

namespace ClinicVets.Desktop;

public partial class MainWindow : Window
{
    private Employee? _currentEmployee;

    public MainWindow()
    {
        InitializeComponent();
        ShowLogin();
    }

    private void ShowLogin(string statusMessage = "")
    {
        DemoModeSession.Exit();
        AppServices.ExitDemoMode();
        Title = "ClinicVets";

        var loginView = string.IsNullOrWhiteSpace(statusMessage) ? new LoginView() : new LoginView(statusMessage);
        loginView.LoginSucceeded += e => { _currentEmployee = e; ShowMainMenu(); };
        loginView.RegisterRequested += ShowRegisterEmployee;
        loginView.ForgotPasswordRequested += ShowForgotPassword;
        loginView.DemoModeRequested += (admin, demoRole) =>
        {
            DemoModeSession.Enter();
            DemoModeSession.SetSimulatedRole(demoRole);
            _currentEmployee = admin;
            Title = "ClinicVets — Demo Mode";
            ShowMainMenu();
        };
        ShowPage(loginView);
    }

    private void ShowRegisterEmployee()
    {
        var v = new RegisterEmployeeView();
        v.BackToLogin += () => ShowLogin();
        v.RegistrationCompleted += msg => ShowLogin(msg);
        ShowPage(v);
    }

    private void ShowForgotPassword()
    {
        var v = new ForgotPasswordView();
        v.BackToLogin += () => ShowLogin();
        v.PasswordResetCompleted += msg => ShowLogin(msg);
        ShowPage(v);
    }

    private void ShowMainMenu()
    {
        if (_currentEmployee is null) { ShowLogin(); return; }
        var v = new MainMenuView(_currentEmployee);
        v.OpenClients += ShowClients;
        v.OpenAnimals += ShowAnimals;
        v.OpenVisits += ShowVisits;
        v.OpenMedicines += ShowMedications;
        v.Logout += () =>
        {
            _currentEmployee = null;
            ShowLogin();
        };
        ShowPage(v);
    }

    private void ShowClients() { if (CanOpen("Clients")) { var v = new ClientsView(); v.BackToMainMenu += ShowMainMenu; ShowPage(v); } }
    private void ShowAnimals() { if (CanOpen("Animals")) { var v = new AnimalsView(); v.BackToMainMenu += ShowMainMenu; ShowPage(v); } }
    private void ShowVisits() { if (CanOpen("Visits")) { var v = new VisitsView(); v.BackToMainMenu += ShowMainMenu; ShowPage(v); } }
    private void ShowMedications() { if (CanOpen("Medications")) { var v = new MedicationsView(); v.BackToMainMenu += ShowMainMenu; ShowPage(v); } }

    private void ShowPage(Control page) { MainContent.Child = page; page.Focus(); }

    private Employee EffectiveEmployee =>
        _currentEmployee is null
            ? null!
            : DemoModeSession.IsActive
                ? DemoModeSession.GetEffectiveEmployee(_currentEmployee)
                : _currentEmployee;

    private bool CanOpen(string screen)
    {
        if (_currentEmployee is null) { ShowLogin(); return false; }
        var employee = EffectiveEmployee;
        var ok = screen switch
        {
            "Clients" => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerSearch) ||
                         RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerRegistration),
            "Animals" => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerAnimals),
            "Visits" => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Visits),
            "Medications" => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Treatments),
            _ => false
        };
        if (!ok) { UIHelper.ShowMessage(this, "אין הרשאה לפתוח מסך זה לפי התפקיד שלך"); ShowMainMenu(); }
        return ok;
    }
}
