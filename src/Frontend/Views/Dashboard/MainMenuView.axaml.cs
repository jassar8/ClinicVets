using ClinicVets.Desktop.Services;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Helpers.Stability;
using ClinicVets.Desktop.Views.Auth;

namespace ClinicVets.Desktop.Views.Dashboard;

public partial class MainMenuView : UserControl
{
    private readonly Employee _employee;
    private readonly DispatcherTimer _clock = new();
    private bool _syncingDemoRoleCombo;

    private Employee EffectiveEmployee =>
        DemoModeSession.IsActive
            ? DemoModeSession.GetEffectiveEmployee(_employee)
            : _employee;

    public Action? OpenClients;
    public Action? OpenAnimals;
    public Action? OpenVisits;
    public Action? OpenMedicines;
    public Action? OpenBills;
    public Action? OpenReports;
    public Action? OpenSettings;
    public Action? Logout;

    public MainMenuView(Employee employee)
    {
        InitializeComponent();
        _employee = employee;
        _clock.Interval = TimeSpan.FromSeconds(1);
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
        Loaded += async (_, _) => await ApplyEmployeeDataAsync();
        UpdateClock();
    }

    private void UpdateClock() =>
        LiveClockText.Text = DateTime.Now.ToString("dddd dd/MM/yyyy HH:mm:ss");

    private async Task ApplyEmployeeDataAsync() =>
        await SafeViewLoader.RunSafeAsync(this, ApplyEmployeeDataCoreAsync, "MainMenu.LoadDashboard");

    private async Task ApplyEmployeeDataCoreAsync()
    {
        var effective = EffectiveEmployee;
        var display = string.IsNullOrWhiteSpace(_employee.Username) ? _employee.FullName : _employee.Username;
        var roleLabel = DemoModeSession.IsActive
            ? $"{GetRoleText(effective.Role)} (???)"
            : GetRoleText(effective.Role);
        LoggedInText.Text = $"????? ?: {display} | ?????: {roleLabel}";

        ConfigureDemoRolePanel();

        var customers = await AppServices.Customers.ListCustomersAsync();
        ClientsCountText.Text = customers.Count.ToString();

        var animalCount = 0;
        foreach (var c in customers)
            animalCount += (await AppServices.Customers.GetAnimalsForCustomerAsync(c.Id)).Count;
        AnimalsCountText.Text = animalCount.ToString();

        var meds = await AppServices.Medications.SearchAsync(null, MedicationSearchFilter.FilterAll);
        MedicationsCountText.Text = meds.Count.ToString();
        var visits = await AppServices.Visits.GetAllAsync();
        VisitsCountText.Text = visits.Count.ToString();

        if (RolePermissions.IsAdministrator(effective))
            ApplyAdminDashboard();
        else if (EmployeeRoleNames.TryParse(effective.Role, out var role))
        {
            if (role == EmployeeRole.Secretary)
                ApplySecretaryDashboard();
            else if (role == EmployeeRole.Veterinarian)
                ApplyVetDashboard();
            else
                ApplyNoAccessDashboard();
        }
        else
            ApplyNoAccessDashboard();
    }

    private void ApplyAdminDashboard()
    {
        RoleBadgeText.Text = "????/? | ???? ????";
        HeroTitleText.Text = $"???? {GetDisplayName()}";
        HeroSubtitleText.Text = "???? ??? ???? ??????.";
        SetSection(ClientsButton, ClientsCardButton, ClientsStatCard, true);
        SetSection(AnimalsButton, AnimalsCardButton, AnimalsStatCard, true);
        SetSection(VisitsButton, VisitsCardButton, VisitsStatCard, true);
        SetSection(MedicinesButton, MedicinesCardButton, MedicationsStatCard, true);
        SetMenuOnly(BillsButton, true);
        SetMenuOnly(ReportsButton, true);
        SetMenuOnly(SettingsButton, true);
    }

    private void ApplySecretaryDashboard()
    {
        RoleBadgeText.Text = "?????/? | ????? ?????? ????? ????";
        HeroTitleText.Text = $"???? {GetDisplayName()}";
        HeroSubtitleText.Text = "?? ???? ????????. ??????? ?????, ??? ?????? ????? ????? ???.";
        SetSection(ClientsButton, ClientsCardButton, ClientsStatCard, true);
        SetSection(AnimalsButton, AnimalsCardButton, AnimalsStatCard, true);
        SetSection(VisitsButton, VisitsCardButton, VisitsStatCard, false);
        SetSection(MedicinesButton, MedicinesCardButton, MedicationsStatCard, false);
        SetMenuOnly(BillsButton, true);
        SetMenuOnly(ReportsButton, false);
        SetMenuOnly(SettingsButton, false);
    }

    private void ApplyVetDashboard()
    {
        RoleBadgeText.Text = "??????/?? | ?????, ??????? ???????";
        HeroTitleText.Text = $"???? {GetDisplayName()}";
        HeroSubtitleText.Text = "?? ???? ????????. ??????? ?????? ????, ??? ?????? ????? ?? ?????.";
        SetSection(ClientsButton, ClientsCardButton, ClientsStatCard, false);
        SetSection(AnimalsButton, AnimalsCardButton, AnimalsStatCard, true);
        SetSection(VisitsButton, VisitsCardButton, VisitsStatCard, true);
        SetSection(MedicinesButton, MedicinesCardButton, MedicationsStatCard, true);
        SetMenuOnly(BillsButton, false);
        SetMenuOnly(ReportsButton, false);
        SetMenuOnly(SettingsButton, false);
    }

    private void ApplyNoAccessDashboard()
    {
        RoleBadgeText.Text = "??? ?????? ??????";
        HeroTitleText.Text = "??? ?????? ?????? ???";
        HeroSubtitleText.Text = "??? ????? ?????? ??? ?????? ????? ????.";
        SetSection(ClientsButton, ClientsCardButton, ClientsStatCard, false);
        SetSection(AnimalsButton, AnimalsCardButton, AnimalsStatCard, false);
        SetSection(VisitsButton, VisitsCardButton, VisitsStatCard, false);
        SetSection(MedicinesButton, MedicinesCardButton, MedicationsStatCard, false);
        SetMenuOnly(BillsButton, false);
        SetMenuOnly(ReportsButton, false);
        SetMenuOnly(SettingsButton, false);
    }

    private string GetDisplayName() =>
        string.IsNullOrWhiteSpace(_employee.Username) ? _employee.FullName : _employee.Username;

    private static void SetSection(Control menu, Control card, Control stat, bool on)
    {
        menu.IsVisible = menu.IsEnabled = card.IsVisible = card.IsEnabled = stat.IsVisible = on;
    }

    private static void SetMenuOnly(Control menu, bool on) =>
        menu.IsVisible = menu.IsEnabled = on;

    private void Clients_Click(object? sender, RoutedEventArgs e) => OpenClients?.Invoke();
    private void Animals_Click(object? sender, RoutedEventArgs e) => OpenAnimals?.Invoke();
    private void Visits_Click(object? sender, RoutedEventArgs e) => OpenVisits?.Invoke();
    private void Medicines_Click(object? sender, RoutedEventArgs e) => OpenMedicines?.Invoke();
    private void Bills_Click(object? sender, RoutedEventArgs e) => OpenBills?.Invoke();
    private void Reports_Click(object? sender, RoutedEventArgs e) => OpenReports?.Invoke();
    private void Settings_Click(object? sender, RoutedEventArgs e) => OpenSettings?.Invoke();
    private void Logout_Click(object? sender, RoutedEventArgs e) => Logout?.Invoke();

    private void ConfigureDemoRolePanel()
    {
        var show = AppServices.IsDemoMode && DesktopBuildOptions.EnableDemoMode;
        DemoRolePanel.IsVisible = show;
        if (!show)
            return;

        _syncingDemoRoleCombo = true;
        DemoRoleCombo.SelectedIndex = DemoModeSession.SimulatedRole switch
        {
            EmployeeRole.Veterinarian => 1,
            EmployeeRole.Secretary => 2,
            _ => 0
        };
        _syncingDemoRoleCombo = false;
    }

    private async void DemoRole_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (_syncingDemoRoleCombo || !AppServices.IsDemoMode || !DesktopBuildOptions.EnableDemoMode)
            return;

        DemoModeSession.SetSimulatedRole(LoginView.GetDemoRoleFromSelectorIndex(DemoRoleCombo.SelectedIndex));
        await ApplyEmployeeDataAsync();
    }

    private static string GetRoleText(string role) =>
        EmployeeRoleNames.TryParse(role, out var r)
            ? r switch
            {
                EmployeeRole.Veterinarian => "??????/??",
                EmployeeRole.Admin => "????/?",
                _ => "?????/?"
            }
            : role;
}
