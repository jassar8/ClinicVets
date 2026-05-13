using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core;
using ClinicVets.Core.Entities;

namespace ClinicVets.Wpf.Views;

public partial class ShellView : UserControl
{
    private static readonly (string Caption, ClinicShellNavKind Kind)[] NavOrder =
    [
        ("Dashboard", ClinicShellNavKind.Dashboard),
        ("Customers", ClinicShellNavKind.Customers),
        ("Animals", ClinicShellNavKind.Animals),
        ("Visits", ClinicShellNavKind.Visits),
        ("Treatments", ClinicShellNavKind.Treatments),
        ("Users & employees", ClinicShellNavKind.UsersEmployees),
        ("Pending approvals", ClinicShellNavKind.PendingApprovals),
        ("Settings", ClinicShellNavKind.Settings)
    ];

    private readonly Employee _sessionEmployee;
    private readonly bool _isQuickAccessDemo;
    private readonly IEmployeeRepository _repository;
    private readonly CustomerDirectoryService _customers;
    private readonly EmployeeApprovalService _approvals;
    private readonly MainWindow _shell;

    private readonly Dictionary<ClinicShellNavKind, Button> _navButtons = new();
    private readonly Dictionary<ClinicShellNavKind, UIElement> _pageCache = new();

    private ClinicShellNavKind _current = ClinicShellNavKind.Dashboard;
    private bool _demoRoleInit;
    private readonly DispatcherTimer _clock = new() { Interval = TimeSpan.FromSeconds(30) };

    public ShellView(
        Employee sessionEmployee,
        bool isQuickAccessDemo,
        IEmployeeRepository repository,
        CustomerDirectoryService customers,
        EmployeeApprovalService approvals,
        MainWindow shell)
    {
        InitializeComponent();
        _sessionEmployee = sessionEmployee;
        _isQuickAccessDemo = isQuickAccessDemo;
        _repository = repository;
        _customers = customers;
        _approvals = approvals;
        _shell = shell;

        Loaded += OnShellLoaded;
        Unloaded += (_, _) => _clock.Stop();
    }

    private void OnShellLoaded(object sender, RoutedEventArgs e)
    {
        Loaded -= OnShellLoaded;
        if (_isQuickAccessDemo)
        {
            DemoBanner.Visibility = Visibility.Visible;
            if (DesktopBuildOptions.EnableDemoMode)
            {
                DemoRoleRow.Visibility = Visibility.Visible;
                DemoRoleCombo.Items.Clear();
                DemoRoleCombo.Items.Add("Administrator");
                DemoRoleCombo.Items.Add("Secretary");
                DemoRoleCombo.Items.Add("Veterinarian");
                _demoRoleInit = true;
                DemoRoleCombo.SelectedIndex = 0;
                DemoModeSession.SetSimulatedRole(EmployeeRole.Admin);
                _demoRoleInit = false;
            }
        }

        ApplyNav();
        _clock.Tick += (_, _) => UpdateClock();
        _clock.Start();
        UpdateClock();
        Navigate(ClinicShellNavKind.Dashboard);
        _ = RefreshPendingBadgeAsync();
    }

    private void ApplyNav()
    {
        NavPanel.Children.Clear();
        _navButtons.Clear();
        foreach (var (caption, kind) in NavOrder)
        {
            if (!ShellNavPermissions.CanAccess(_sessionEmployee, kind))
                continue;
            var btn = new Button
            {
                Content = caption,
                Tag = kind,
                Margin = new Thickness(0, 4, 0, 0)
            };
            btn.Click += NavButtonClick;
            SetNavStyle(btn, active: false);
            NavPanel.Children.Add(btn);
            _navButtons[kind] = btn;
        }
    }

    private void SetNavStyle(Button btn, bool active)
    {
        var styleKey = active ? "Btn.NavActive" : "Btn.Nav";
        btn.Style = (Style)FindResource(styleKey);
    }

    private void NavButtonClick(object sender, RoutedEventArgs e)
    {
        if (sender is not Button b || b.Tag is not ClinicShellNavKind kind)
            return;
        Navigate(kind);
    }

    private void Navigate(ClinicShellNavKind kind)
    {
        _current = kind;
        foreach (var kv in _navButtons)
            SetNavStyle(kv.Value, kv.Key == kind);

        UpdateHeader(kind);
        PageHost.Content = ResolvePage(kind);
    }

    private void UpdateHeader(ClinicShellNavKind kind)
    {
        var eff = DemoModeSession.GetEffectiveEmployee(_sessionEmployee);
        var roleText = EmployeeRoleNames.TryParse(eff.Role, out var pr)
            ? (pr == EmployeeRole.Admin ? "Administrator" : EmployeeRoleNames.ToStoredString(pr))
            : eff.Role;

        var (title, sub) = kind switch
        {
            ClinicShellNavKind.Dashboard => ($"Welcome back, {FirstName(_sessionEmployee)}", "Here's what's happening at your clinic today."),
            ClinicShellNavKind.Customers => ("Customers", "Search and register pet owners"),
            ClinicShellNavKind.Animals => ("Animals", "Household pets linked to customer records"),
            ClinicShellNavKind.Visits => ("Visits", "Clinical visits and scheduling"),
            ClinicShellNavKind.Treatments => ("Treatments", "Treatment history and protocols"),
            ClinicShellNavKind.UsersEmployees => ("Users & employees", "Directory and account actions"),
            ClinicShellNavKind.PendingApprovals => ("Pending approvals", "Assign Employee IDs and approve roles"),
            ClinicShellNavKind.Settings => ("Settings", "Clinic configuration"),
            _ => ("ClinicVets", "")
        };
        HeaderTitle.Text = title;
        HeaderSubtitle.Text = DemoModeSession.IsActive && DemoModeSession.SimulatedRole.HasValue
            ? $"{sub} · Viewing as: {roleText}"
            : sub;
    }

    private static string FirstName(Employee e)
    {
        var parts = e.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "there";
    }

    private void UpdateClock() =>
        ClockText.Text = DateTime.Now.ToString("dddd, MMM d · h:mm tt");

    private UIElement ResolvePage(ClinicShellNavKind kind)
    {
        if (_pageCache.TryGetValue(kind, out var cached))
            return cached;

        var eff = DemoModeSession.GetEffectiveEmployee(_sessionEmployee);
        UIElement page = kind switch
        {
            ClinicShellNavKind.Dashboard => new DashboardHomeView(
                _sessionEmployee,
                ShellNavPermissions.CanAccess(_sessionEmployee, ClinicShellNavKind.Customers) ? _customers : null),
            ClinicShellNavKind.Customers => new CustomersHubView(
                _customers,
                RolePermissions.CanAccessDashboardSection(eff, DashboardSection.CustomerSearch),
                RolePermissions.CanAccessDashboardSection(eff, DashboardSection.CustomerRegistration)),
            ClinicShellNavKind.Animals => new AnimalsView(_customers),
            ClinicShellNavKind.Visits => _isQuickAccessDemo
                ? new PlaceholderView(
                    "Visits",
                    "Demo schedule: morning consults, afternoon procedures. Full scheduling will connect here in a future release.")
                : new PlaceholderView(
                    "Visits",
                    "Scheduling, check-in, and visit documentation will be added here in a future iteration."),
            ClinicShellNavKind.Treatments => new PlaceholderView(
                "Treatments",
                "Treatment plans and protocols will be managed from this workspace once implemented."),
            ClinicShellNavKind.UsersEmployees => new AdminUsersView(
                _sessionEmployee,
                _repository,
                _approvals,
                UsersHubTab.All,
                OnStaffDirectoryChanged),
            ClinicShellNavKind.PendingApprovals => new AdminUsersView(
                _sessionEmployee,
                _repository,
                _approvals,
                UsersHubTab.Pending,
                OnStaffDirectoryChanged),
            ClinicShellNavKind.Settings => new PlaceholderView(
                "Settings",
                "Clinic-wide preferences and integrations will appear here in a future release."),
            _ => new PlaceholderView("ClinicVets", "Select an item from the sidebar.")
        };

        _pageCache[kind] = page;
        return page;
    }

    private void OnStaffDirectoryChanged() => _ = RefreshPendingBadgeAsync();

    private async Task RefreshPendingBadgeAsync()
    {
        try
        {
            if (!_navButtons.TryGetValue(ClinicShellNavKind.PendingApprovals, out var btn))
                return;
            if (!RolePermissions.IsAdministrator(DemoModeSession.GetEffectiveEmployee(_sessionEmployee)))
            {
                btn.Content = "Pending approvals";
                return;
            }

            var pending = await _repository.GetPendingRegistrationsAsync();
            btn.Content = pending.Count > 0 ? $"Pending approvals ({pending.Count})" : "Pending approvals";
        }
        catch
        {
            // ignore
        }
    }

    private void ClearRolePages() => _pageCache.Clear();

    private void DemoRoleCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isQuickAccessDemo || !DesktopBuildOptions.EnableDemoMode || _demoRoleInit || DemoRoleCombo.SelectedIndex < 0)
            return;

        var role = DemoRoleCombo.SelectedIndex switch
        {
            0 => EmployeeRole.Admin,
            1 => EmployeeRole.Secretary,
            _ => EmployeeRole.Veterinarian
        };
        DemoModeSession.SetSimulatedRole(role);
        ClearRolePages();
        ApplyNav();
        var target = _current;
        if (!ShellNavPermissions.CanAccess(_sessionEmployee, target))
            Navigate(ClinicShellNavKind.Dashboard);
        else
            Navigate(target);
    }

    private void OnLogout(object sender, RoutedEventArgs e) => _shell.ShowLogin();
}
