using System.Drawing;
using System.Drawing.Drawing2D;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Unified post-login shell: dark teal sidebar, welcome header with clock, and card workspace for every role.
/// </summary>
public sealed class ClinicShellView : UserControl
{
    private readonly Employee _employee;
    private readonly MainShellForm _shell;
    private readonly EmployeeRegistrationService _registration;
    private readonly EmployeeApprovalService _approvals;
    private readonly IEmployeeRepository _repository;
    private readonly CustomerDirectoryService _customerDirectory;
    private readonly bool _isQuickAccessDemo;

    private readonly Dictionary<ClinicShellNavKind, ModernSidebarNavButton> _navItems = new();
    private readonly Dictionary<ClinicShellNavKind, Control> _lazyPages = new();

    private readonly Label _headerTitle = new();
    private readonly Label _headerSubtitle = new();
    private readonly Label _clock = new();
    private readonly Panel _workspace = new() { Dock = DockStyle.Fill, BackColor = UiTheme.ContentCanvas, Padding = new Padding(UiTheme.Layout.PageGutter, UiTheme.Layout.CardInset, UiTheme.Layout.PageGutter, UiTheme.Layout.CardInset) };
    private readonly ModernCardPanel _workspaceCard = new() { Dock = DockStyle.Fill, Padding = new Padding(2) };

    private AdminUsersManagementPanel? _usersHub;
    private ModernSidebarNavButton? _navPending;
    private StaffHomeDashboardPanel? _homePanel;
    private readonly System.Windows.Forms.Timer _clockTimer = new() { Interval = 30_000 };

    private ComboBox? _demoRoleCombo;
    private bool _demoRoleComboInit;
    private Label? _profileNameLabel;
    private Label? _profileRoleLabel;
    private Panel? _profileAvatar;

    public ClinicShellView(
        Employee employee,
        MainShellForm shell,
        EmployeeRegistrationService registration,
        EmployeeApprovalService approvals,
        IEmployeeRepository repository,
        CustomerDirectoryService customerDirectory,
        bool isQuickAccessDemo = false)
    {
        _employee = employee;
        _shell = shell;
        _registration = registration;
        _approvals = approvals;
        _repository = repository;
        _customerDirectory = customerDirectory;
        _isQuickAccessDemo = isQuickAccessDemo;

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
        Dock = DockStyle.Fill;
        Font = shell.Font;
        BackColor = UiTheme.PageBackground;
        Paint += PaintBodyGradient;

        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.PageBackground
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UiTheme.SidebarWidth));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        split.Controls.Add(BuildSidebar(), 0, 0);
        split.Controls.Add(BuildMainColumn(), 1, 0);

        Controls.Add(split);

        _clockTimer.Tick += (_, _) => UpdateClock();
        _clockTimer.Start();

        Load += async (_, _) =>
        {
            InitDemoRoleSwitcherFromShell();
            UpdateClock();
            Navigate(ClinicShellNavKind.Dashboard);
            await RefreshPendingBadgeAsync();
        };
    }

    private Control BuildSidebar()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.AdminSidebarBackground,
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildSidebarBrand(), 0, 0);

        var navHost = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12, 4, 12, 8),
            BackColor = UiTheme.AdminSidebarBackground
        };

        void TryAdd(string caption, ClinicShellNavKind kind)
        {
            if (!ShellNavPermissions.CanAccess(_employee, kind))
                return;
            var row = new ModernSidebarNavButton(caption, kind)
            {
                Height = 46,
                Margin = new Padding(6, 2, 6, 2)
            };
            row.Click += (_, _) => Navigate(kind);
            _navItems[kind] = row;
            navHost.Controls.Add(row);
            if (kind == ClinicShellNavKind.PendingApprovals)
                _navPending = row;
        }

        TryAdd("Dashboard", ClinicShellNavKind.Dashboard);
        TryAdd("Customers", ClinicShellNavKind.Customers);
        TryAdd("Animals", ClinicShellNavKind.Animals);
        TryAdd("Visits", ClinicShellNavKind.Visits);
        TryAdd("Treatments", ClinicShellNavKind.Treatments);
        TryAdd("Users & employees", ClinicShellNavKind.UsersEmployees);
        TryAdd("Pending approvals", ClinicShellNavKind.PendingApprovals);
        TryAdd("Settings", ClinicShellNavKind.Settings);

        void SyncNav() => ResponsiveLayout.SyncSidebarNavButtonWidths(navHost);
        navHost.SizeChanged += (_, _) => SyncNav();
        navHost.HandleCreated += (_, _) => SyncNav();

        root.Controls.Add(navHost, 0, 1);

        var bottom = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Padding = new Padding(12, 4, 12, 12),
            BackColor = UiTheme.AdminSidebarBackground,
            Height = 56
        };
        bottom.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var logout = new Button
        {
            Text = "Logout",
            Dock = DockStyle.Fill,
            Height = 44,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            Cursor = Cursors.Hand,
            TabStop = true
        };
        logout.FlatAppearance.BorderColor = UiTheme.SidebarLogoutBorder;
        logout.FlatAppearance.BorderSize = 1;
        logout.BackColor = UiTheme.SidebarLogoutBackground;
        logout.Click += (_, _) => _shell.NavigateToLogin();
        bottom.Controls.Add(logout, 0, 0);

        root.Controls.Add(bottom, 0, 2);
        return root;
    }

    private TableLayoutPanel BuildSidebarBrand()
    {
        var brand = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(14, 16, 14, 10),
            BackColor = UiTheme.AdminSidebarBackground
        };
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 52F));
        brand.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        brand.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        brand.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var logo = new PictureBox
        {
            Size = new Size(40, 40),
            SizeMode = PictureBoxSizeMode.Zoom,
            Margin = new Padding(0, 2, 0, 0),
            BackColor = UiTheme.AdminSidebarBackground
        };
        try
        {
            logo.Image = AppBranding.GetHeaderImage();
        }
        catch
        {
            logo.Visible = false;
        }

        var title = new Label
        {
            Text = "ClinicVets",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.White,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            BackColor = UiTheme.AdminSidebarBackground,
            UseCompatibleTextRendering = false
        };
        var sub = new Label
        {
            Text = "Veterinary clinic system",
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.SidebarTextMutedOnDark,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            MaximumSize = new Size(220, 0),
            BackColor = UiTheme.AdminSidebarBackground,
            UseCompatibleTextRendering = false
        };

        brand.Controls.Add(logo, 0, 0);
        brand.SetRowSpan(logo, 2);
        brand.Controls.Add(title, 1, 0);
        brand.Controls.Add(sub, 1, 1);
        return brand;
    }

    private Control BuildMainColumn()
    {
        var col = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = _isQuickAccessDemo ? 3 : 2,
            ColumnCount = 1,
            BackColor = UiTheme.ContentCanvas
        };
        if (_isQuickAccessDemo)
            col.RowStyles.Add(new RowStyle(SizeType.Absolute, DesktopBuildOptions.EnableDemoMode ? 54F : 36F));
        col.RowStyles.Add(new RowStyle(SizeType.Absolute, UiTheme.Layout.HeaderMinHeight + 8F));
        col.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var headerRow = _isQuickAccessDemo ? 1 : 0;
        var workspaceRow = _isQuickAccessDemo ? 2 : 1;

        if (_isQuickAccessDemo)
        {
            var demoStrip = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                BackColor = UiTheme.WarningBackground,
                Padding = new Padding(14, 4, 14, 4),
                ColumnCount = 1,
                RowCount = DesktopBuildOptions.EnableDemoMode ? 2 : 1
            };
            demoStrip.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            if (DesktopBuildOptions.EnableDemoMode)
                demoStrip.RowStyles.Add(new RowStyle(SizeType.AutoSize));

            demoStrip.Controls.Add(
                new Label
                {
                    Text = "Demo Mode — not real login · in-memory data only",
                    Dock = DockStyle.Fill,
                    TextAlign = ContentAlignment.MiddleLeft,
                    ForeColor = UiTheme.WarningText,
                    Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
                    BackColor = UiTheme.WarningBackground,
                    AutoSize = true
                },
                0,
                0);

            if (DesktopBuildOptions.EnableDemoMode)
            {
                var roleRow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    AutoSize = true,
                    BackColor = UiTheme.WarningBackground,
                    Padding = new Padding(0, 2, 0, 0)
                };
                roleRow.Controls.Add(
                    new Label
                    {
                        Text = "Demo role:",
                        AutoSize = true,
                        Margin = new Padding(0, 6, 8, 0),
                        ForeColor = UiTheme.WarningText,
                        Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                        BackColor = UiTheme.WarningBackground
                    });
                _demoRoleCombo = new ComboBox
                {
                    DropDownStyle = ComboBoxStyle.DropDownList,
                    Width = 168,
                    Margin = new Padding(0, 2, 0, 0),
                    Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
                    FlatStyle = FlatStyle.Flat
                };
                _demoRoleCombo.Items.AddRange(new object[] { "Administrator", "Secretary", "Veterinarian" });
                _demoRoleCombo.SelectedIndexChanged += OnDemoRoleComboChanged;
                roleRow.Controls.Add(_demoRoleCombo);
                demoStrip.Controls.Add(roleRow, 0, 1);
            }

            col.Controls.Add(demoStrip, 0, 0);
        }

        var header = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.CardWhite,
            Padding = new Padding(20, 10, 20, 10),
            ColumnCount = 3,
            RowCount = 1
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        header.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };

        var titleStack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.CardWhite
        };
        titleStack.RowStyles.Add(new RowStyle(SizeType.Percent, 55F));
        titleStack.RowStyles.Add(new RowStyle(SizeType.Percent, 45F));

        _headerTitle.Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point);
        _headerTitle.ForeColor = UiTheme.TextDark;
        _headerTitle.AutoSize = false;
        _headerTitle.AutoEllipsis = true;
        _headerTitle.Dock = DockStyle.Fill;
        _headerTitle.TextAlign = ContentAlignment.MiddleLeft;
        _headerTitle.BackColor = UiTheme.CardWhite;
        _headerTitle.UseCompatibleTextRendering = false;

        _headerSubtitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        _headerSubtitle.ForeColor = UiTheme.TextMuted;
        _headerSubtitle.AutoSize = false;
        _headerSubtitle.AutoEllipsis = true;
        _headerSubtitle.Dock = DockStyle.Fill;
        _headerSubtitle.TextAlign = ContentAlignment.TopLeft;
        _headerSubtitle.BackColor = UiTheme.CardWhite;
        _headerSubtitle.UseCompatibleTextRendering = false;

        titleStack.Controls.Add(_headerTitle, 0, 0);
        titleStack.Controls.Add(_headerSubtitle, 0, 1);

        _clock.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point);
        _clock.ForeColor = UiTheme.TextMuted;
        _clock.AutoSize = false;
        _clock.Dock = DockStyle.Fill;
        _clock.Margin = new Padding(0, 0, 16, 0);
        _clock.TextAlign = ContentAlignment.MiddleRight;
        _clock.BackColor = UiTheme.CardWhite;
        _clock.UseCompatibleTextRendering = false;

        var profile = BuildProfileChip();
        profile.Margin = new Padding(0, 0, 0, 0);

        header.Controls.Add(titleStack, 0, 0);
        header.Controls.Add(_clock, 1, 0);
        header.Controls.Add(profile, 2, 0);

        _workspace.Controls.Add(_workspaceCard);

        col.Controls.Add(header, 0, headerRow);
        col.Controls.Add(_workspace, 0, workspaceRow);
        return col;
    }

    private void UpdateClock() =>
        _clock.Text = $"{DateTime.Now:MMMM d, yyyy}   |   {DateTime.Now:h:mm tt}";

    private TableLayoutPanel BuildProfileChip()
    {
        var wrap = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 1,
            BackColor = UiTheme.CardWhite
        };
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 48F));
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        wrap.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 36F));
        wrap.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _profileAvatar = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 4, 8, 4),
            BackColor = UiTheme.AccentMintSoft
        };
        _profileAvatar.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var letter = string.IsNullOrWhiteSpace(_employee.Username)
                ? (_employee.FullName.Length > 0 ? _employee.FullName[0].ToString() : "?")
                : _employee.Username.Trim()[0].ToString().ToUpperInvariant();
            using var path = UiChrome.CreateRoundRectPath(new Rectangle(0, 0, _profileAvatar.Width - 1, _profileAvatar.Height - 1), 20);
            using (var b = new SolidBrush(UiTheme.PrimaryButton))
                e.Graphics.FillPath(b, path);
            TextRenderer.DrawText(
                e.Graphics,
                letter,
                new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point),
                _profileAvatar.ClientRectangle,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        var eff = DemoModeSession.GetEffectiveEmployee(_employee);
        var roleText = EmployeeRoleNames.TryParse(eff.Role, out var pr)
            ? (pr == EmployeeRole.Admin ? "Administrator" : EmployeeRoleNames.ToStoredString(pr))
            : eff.Role;

        var textStack = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.CardWhite,
            Margin = new Padding(0, 2, 8, 2)
        };
        textStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        textStack.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _profileNameLabel = new Label
        {
            Text = string.IsNullOrWhiteSpace(_employee.Username) ? _employee.FullName : _employee.Username,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            AutoEllipsis = true,
            MaximumSize = new Size(200, 0),
            BackColor = UiTheme.CardWhite,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            UseCompatibleTextRendering = false
        };
        _profileRoleLabel = new Label
        {
            Text = roleText,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            AutoEllipsis = true,
            MaximumSize = new Size(200, 0),
            BackColor = UiTheme.CardWhite,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            UseCompatibleTextRendering = false
        };
        textStack.Controls.Add(_profileNameLabel, 0, 0);
        textStack.Controls.Add(_profileRoleLabel, 0, 1);

        var menuBtn = new Button
        {
            Text = "▾",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 6, 0, 6),
            FlatStyle = FlatStyle.Flat,
            ForeColor = UiTheme.TextMuted,
            TabStop = false,
            Cursor = Cursors.Hand
        };
        menuBtn.FlatAppearance.BorderSize = 0;
        menuBtn.FlatAppearance.MouseOverBackColor = UiTheme.AccentMintWash;
        var menu = new ContextMenuStrip();
        menu.Items.Add("Sign out", null, (_, _) => _shell.NavigateToLogin());
        menuBtn.Click += (_, _) => menu.Show(menuBtn, new Point(0, menuBtn.Height));

        wrap.Controls.Add(_profileAvatar, 0, 0);
        wrap.Controls.Add(textStack, 1, 0);
        wrap.Controls.Add(menuBtn, 2, 0);
        return wrap;
    }

    private void InitDemoRoleSwitcherFromShell()
    {
        if (!_isQuickAccessDemo || !DesktopBuildOptions.EnableDemoMode || _demoRoleCombo is null)
            return;
        _demoRoleComboInit = true;
        _demoRoleCombo.SelectedIndex = 0;
        DemoModeSession.SetSimulatedRole(EmployeeRole.Admin);
        _demoRoleComboInit = false;
        ApplyDemoNavVisibility();
    }

    private void OnDemoRoleComboChanged(object? sender, EventArgs e)
    {
        if (!_isQuickAccessDemo || _demoRoleComboInit || _demoRoleCombo is null)
            return;

        var role = _demoRoleCombo.SelectedIndex switch
        {
            0 => EmployeeRole.Admin,
            1 => EmployeeRole.Secretary,
            _ => EmployeeRole.Veterinarian
        };
        DemoModeSession.SetSimulatedRole(role);
        ApplyDemoNavVisibility();
        ClearRoleDependentPages();

        ClinicShellNavKind target = ClinicShellNavKind.Dashboard;
        foreach (var kv in _navItems)
        {
            if (!kv.Value.IsActive)
                continue;
            target = kv.Key;
            break;
        }

        if (!ShellNavPermissions.CanAccess(_employee, target))
            Navigate(ClinicShellNavKind.Dashboard);
        else
            Navigate(target);

        RefreshProfileLabels();
        _profileAvatar?.Invalidate();
        _ = RefreshPendingBadgeAsync();
    }

    private void ApplyDemoNavVisibility()
    {
        if (!_isQuickAccessDemo || !DesktopBuildOptions.EnableDemoMode)
            return;
        foreach (var kv in _navItems)
            kv.Value.Visible = ShellNavPermissions.CanAccess(_employee, kv.Key);
    }

    private void ClearRoleDependentPages()
    {
        foreach (var c in _lazyPages.Values)
            c.Dispose();
        _lazyPages.Clear();

        if (_usersHub is not null)
        {
            _usersHub.StaffDirectoryChanged -= OnUsersHubStaffDirectoryChanged;
            _usersHub.Dispose();
            _usersHub = null;
        }

        _homePanel?.Dispose();
        _homePanel = null;
    }

    private async void OnUsersHubStaffDirectoryChanged(object? sender, EventArgs e) =>
        await RefreshPendingBadgeAsync();

    private void RefreshProfileLabels()
    {
        if (_profileRoleLabel is null)
            return;
        var eff = DemoModeSession.GetEffectiveEmployee(_employee);
        var roleText = EmployeeRoleNames.TryParse(eff.Role, out var pr)
            ? (pr == EmployeeRole.Admin ? "Administrator" : EmployeeRoleNames.ToStoredString(pr))
            : eff.Role;
        _profileRoleLabel.Text = roleText;
    }

    private void Navigate(ClinicShellNavKind kind)
    {
        foreach (var kv in _navItems)
            kv.Value.IsActive = kv.Key == kind;

        UpdateHeaderCopy(kind);
        while (_workspaceCard.Controls.Count > 0)
        {
            var c = _workspaceCard.Controls[0];
            _workspaceCard.Controls.Remove(c);
        }

        var page = ResolvePage(kind);
        page.Dock = DockStyle.Fill;
        _workspaceCard.Controls.Add(page);

        if (kind == ClinicShellNavKind.Dashboard && _homePanel is not null)
            _ = _homePanel.RefreshMetricsAsync();
    }

    private Control ResolvePage(ClinicShellNavKind kind)
    {
        var eff = DemoModeSession.GetEffectiveEmployee(_employee);
        switch (kind)
        {
            case ClinicShellNavKind.Dashboard:
                if (_homePanel is null)
                {
                    CustomerDirectoryService? cust = ShellNavPermissions.CanAccess(_employee, ClinicShellNavKind.Customers)
                        ? _customerDirectory
                        : null;
                    _homePanel = new StaffHomeDashboardPanel(eff, cust, Navigate);
                }

                return _homePanel;

            case ClinicShellNavKind.UsersEmployees:
            case ClinicShellNavKind.PendingApprovals:
                if (_usersHub is null)
                {
                    _usersHub = new AdminUsersManagementPanel(eff, _repository, _registration, _approvals);
                    _usersHub.StaffDirectoryChanged += OnUsersHubStaffDirectoryChanged;
                }

                var tab = kind == ClinicShellNavKind.PendingApprovals ? UsersHubTab.Pending : UsersHubTab.All;
                _usersHub.SetHubTab(tab);
                return _usersHub;

            case ClinicShellNavKind.Customers:
                return GetLazy(kind, new CustomersHubPanel(_customerDirectory, eff));

            case ClinicShellNavKind.Animals:
                return GetLazy(kind, new CustomerAnimalsPanel(_customerDirectory));

            case ClinicShellNavKind.Visits:
                if (_isQuickAccessDemo)
                {
                    return GetLazy(kind, new DemoVisitsPanel());
                }

                return GetLazy(
                    kind,
                    new AdminPlaceholderPanel(
                        "Visits",
                        "Scheduling, check-in, and visit documentation will be added here in a future iteration."));

            case ClinicShellNavKind.Treatments:
                return GetLazy(
                    kind,
                    new AdminPlaceholderPanel(
                        "Treatments",
                        "Treatment plans and protocols will be managed from this workspace once implemented."));

            case ClinicShellNavKind.Settings:
                return GetLazy(
                    kind,
                    new AdminPlaceholderPanel(
                        "Settings",
                        "Clinic-wide preferences and integrations will appear here in a future release."));

            default:
                return GetLazy(
                    kind,
                    new AdminPlaceholderPanel("ClinicVets", "Select an item from the sidebar."));
        }
    }

    private Control GetLazy(ClinicShellNavKind kind, Control created)
    {
        if (_lazyPages.TryGetValue(kind, out var existing))
            return existing;
        _lazyPages[kind] = created;
        return created;
    }

    private static string FirstName(Employee e)
    {
        var parts = e.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : "there";
    }

    private void UpdateHeaderCopy(ClinicShellNavKind kind)
    {
        var (title, sub) = kind switch
        {
            ClinicShellNavKind.Dashboard => ($"Welcome back, {FirstName(_employee)} 👋", "Here's what's happening at your clinic today."),
            ClinicShellNavKind.Customers => ("Customers", "Search and register pet owners"),
            ClinicShellNavKind.Animals => ("Animals", "Household pets linked to customer records"),
            ClinicShellNavKind.Visits => ("Visits", "Clinical visits and scheduling"),
            ClinicShellNavKind.Treatments => ("Treatments", "Treatment history and protocols"),
            ClinicShellNavKind.UsersEmployees => ("Users & employees", "Directory, filters, and account actions"),
            ClinicShellNavKind.PendingApprovals => ("Pending approvals", "Assign Employee IDs and finalize roles before employees can sign in"),
            ClinicShellNavKind.Settings => ("Settings", "Clinic configuration"),
            _ => ("ClinicVets", string.Empty)
        };

        _headerTitle.Text = title;
        _headerSubtitle.Text = sub;
    }

    private async Task RefreshPendingBadgeAsync()
    {
        try
        {
            if (_navPending is null)
                return;
            if (!RolePermissions.IsAdministrator(DemoModeSession.GetEffectiveEmployee(_employee)))
            {
                _navPending.BadgeCount = null;
                _navPending.Invalidate();
                return;
            }

            var pending = await _repository.GetPendingRegistrationsAsync();
            _navPending.BadgeCount = pending.Count > 0 ? pending.Count : null;
            _navPending.Invalidate();
        }
        catch
        {
            // ignore
        }
    }

    private static void PaintBodyGradient(object? sender, PaintEventArgs e)
    {
        if (sender is not Control c)
            return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var brush = new LinearGradientBrush(
            c.ClientRectangle,
            UiTheme.PageBackground,
            UiTheme.PageGradientBottom,
            LinearGradientMode.Vertical);
        g.FillRectangle(brush, c.ClientRectangle);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
            _clockTimer.Dispose();
        base.Dispose(disposing);
    }
}
