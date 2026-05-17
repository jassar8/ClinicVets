using Avalonia.Controls;
using ClinicVets.Application.Security;
using ClinicVets.Application.Shell;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers;
using ClinicVets.Desktop.Stability;
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
        Navigate(AppRouteCatalog.Login, () =>
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
            return loginView;
        });
    }

    private void ShowRegisterEmployee() =>
        Navigate(AppRouteCatalog.RegisterEmployee, () =>
        {
            var v = new RegisterEmployeeView();
            v.BackToLogin += () => ShowLogin();
            v.RegistrationCompleted += msg => ShowLogin(msg);
            return v;
        });

    private void ShowForgotPassword() =>
        Navigate(AppRouteCatalog.ForgotPassword, () =>
        {
            var v = new ForgotPasswordView();
            v.BackToLogin += () => ShowLogin();
            v.PasswordResetCompleted += msg => ShowLogin(msg);
            return v;
        });

    private void ShowMainMenu()
    {
        if (_currentEmployee is null) { ShowLogin(); return; }
        Navigate(AppRouteCatalog.MainMenu, () =>
        {
            var v = new MainMenuView(_currentEmployee);
            v.OpenClients += ShowClients;
            v.OpenAnimals += ShowAnimals;
            v.OpenVisits += ShowVisits;
            v.OpenMedicines += ShowMedications;
            v.OpenBills += ShowBills;
            v.Logout += () =>
            {
                _currentEmployee = null;
                ShowLogin();
            };
            return v;
        });
    }

    private void ShowClients() =>
        NavigateFeature(AppRouteCatalog.Clients, () =>
        {
            var v = new ClientsView();
            v.BackToMainMenu += ShowMainMenu;
            return v;
        });

    private void ShowAnimals() =>
        NavigateFeature(AppRouteCatalog.Animals, () =>
        {
            var v = new AnimalsView();
            v.BackToMainMenu += ShowMainMenu;
            return v;
        });

    private void ShowVisits() =>
        NavigateFeature(AppRouteCatalog.Visits, () =>
        {
            var v = new VisitsView();
            v.BackToMainMenu += ShowMainMenu;
            return v;
        });

    private void ShowMedications() =>
        NavigateFeature(AppRouteCatalog.Medications, () =>
        {
            var v = new MedicationsView();
            v.BackToMainMenu += ShowMainMenu;
            return v;
        });

    private void ShowBills() =>
        NavigateFeature(AppRouteCatalog.Bills, () =>
        {
            var v = new BillsView();
            v.BackToMainMenu += ShowMainMenu;
            return v;
        });

    private void NavigateFeature(string route, Func<Control> factory)
    {
        if (!CanOpen(route))
            return;
        Navigate(route, factory);
    }

    private void Navigate(string route, Func<Control> factory)
    {
        try
        {
            var page = factory();
            ShowPage(page);
            AppStability.Log($"Navigate OK: {route}");
        }
        catch (Exception ex)
        {
            AppStability.LogException($"Navigate:{route}", ex);
            UIHelper.ShowMessage(this, SafeViewLoader.FriendlyMessage(route));
            if (_currentEmployee is not null && route != AppRouteCatalog.MainMenu && route != AppRouteCatalog.Login)
                ShowMainMenu();
        }
    }

    private void ShowPage(Control page)
    {
        MainContent.Child = page;
        page.Focus();
    }

    private Employee EffectiveEmployee =>
        _currentEmployee is null
            ? null!
            : DemoModeSession.IsActive
                ? DemoModeSession.GetEffectiveEmployee(_currentEmployee)
                : _currentEmployee;

    private bool CanOpen(string route)
    {
        if (_currentEmployee is null)
        {
            ShowLogin();
            return false;
        }

        var employee = EffectiveEmployee;
        if (AppRouteCatalog.CanOpenRoute(employee, route))
            return true;

        UIHelper.ShowMessage(this, "אין הרשאה לפתוח מסך זה לפי התפקיד שלך");
        ShowMainMenu();
        return false;
    }
}
