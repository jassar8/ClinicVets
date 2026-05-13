using System.Drawing;
using System.Drawing.Drawing2D;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop;
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

    private readonly Dictionary<ClinicShellNavKind, AdminSidebarNavItem> _navItems = new();
    private readonly Dictionary<ClinicShellNavKind, Control> _lazyPages = new();

    private readonly Label _headerTitle = new();
    private readonly Label _headerSubtitle = new();
    private readonly Label _clock = new();
    private readonly Panel _workspace = new() { Dock = DockStyle.Fill, BackColor = UiTheme.ContentCanvas, Padding = new Padding(24, 20, 24, 24) };
    private readonly Panel _workspaceCard = new() { Dock = DockStyle.Fill, BackColor = UiTheme.CardWhite, Padding = new Padding(4) };

    private AdminUsersManagementPanel? _usersHub;
    private AdminSidebarNavItem? _navPending;
    private StaffHomeDashboardPanel? _homePanel;
    private readonly System.Windows.Forms.Timer _clockTimer = new() { Interval = 30_000 };

    public ClinicShellView(
        Employee employee,
        MainShellForm shell,
        EmployeeRegistrationService registration,
        EmployeeApprovalService approvals,
        IEmployeeRepository repository,
        CustomerDirectoryService customerDirectory)
    {
        _employee = employee;
        _shell = shell;
        _registration = registration;
        _approvals = approvals;
        _repository = repository;
        _customerDirectory = customerDirectory;

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
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280F));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));

        split.Controls.Add(BuildSidebar(), 0, 0);
        split.Controls.Add(BuildMainColumn(), 1, 0);

        Controls.Add(split);

        _clockTimer.Tick += (_, _) => UpdateClock();
        _clockTimer.Start();

        Load += async (_, _) =>
        {
            UpdateClock();
            Navigate(ClinicShellNavKind.Dashboard);
            await RefreshPendingBadgeAsync();
        };
    }

    private Panel BuildSidebar()
    {
        var root = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.AdminSidebarBackground };

        var brand = new Panel
        {
            Dock = DockStyle.Top,
            Height = 120,
            Padding = new Padding(20, 24, 20, 12),
            BackColor = UiTheme.AdminSidebarBackground
        };
        var logo = new PictureBox
        {
            Size = new Size(40, 40),
            Location = new Point(20, 24),
            SizeMode = PictureBoxSizeMode.Zoom,
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
            Font = new Font("Segoe UI", 17F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.White,
            AutoSize = true,
            Location = new Point(logo.Visible ? 68 : 20, 26),
            BackColor = UiTheme.AdminSidebarBackground
        };
        var sub = new Label
        {
            Text = "Veterinary clinic system",
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = Color.FromArgb(200, 228, 222),
            AutoSize = true,
            Location = new Point(logo.Visible ? 68 : 20, 58),
            MaximumSize = new Size(240, 0),
            BackColor = UiTheme.AdminSidebarBackground
        };
        brand.Controls.Add(logo);
        brand.Controls.Add(title);
        brand.Controls.Add(sub);

        var navHost = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12, 8, 12, 12),
            BackColor = UiTheme.AdminSidebarBackground
        };

        void TryAdd(string caption, ClinicShellNavKind kind)
        {
            if (!ShellNavPermissions.CanAccess(_employee, kind))
                return;
            var row = new AdminSidebarNavItem(caption, kind)
            {
                Width = 248,
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

        var bottom = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 72,
            Padding = new Padding(16, 8, 16, 16),
            BackColor = UiTheme.AdminSidebarBackground
        };
        var logout = new Button
        {
            Text = "Logout",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            Cursor = Cursors.Hand,
            TabStop = true
        };
        logout.FlatAppearance.BorderColor = Color.FromArgb(200, 230, 224);
        logout.FlatAppearance.BorderSize = 1;
        logout.BackColor = Color.FromArgb(18, 88, 82);
        logout.Click += (_, _) => _shell.NavigateToLogin();
        bottom.Controls.Add(logout);

        root.Controls.Add(navHost);
        root.Controls.Add(bottom);
        root.Controls.Add(brand);
        return root;
    }

    private Control BuildMainColumn()
    {
        var col = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = UiTheme.ContentCanvas
        };
        col.RowStyles.Add(new RowStyle(SizeType.Absolute, 104F));
        col.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.CardWhite,
            Padding = new Padding(28, 16, 28, 12)
        };
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };

        _headerTitle.Font = new Font("Segoe UI", 21F, FontStyle.Bold, GraphicsUnit.Point);
        _headerTitle.ForeColor = UiTheme.TextDark;
        _headerTitle.AutoSize = true;
        _headerTitle.Location = new Point(28, 12);

        _headerSubtitle.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        _headerSubtitle.ForeColor = UiTheme.TextMuted;
        _headerSubtitle.AutoSize = true;
        _headerSubtitle.Location = new Point(28, 48);
        _headerSubtitle.MaximumSize = new Size(520, 0);

        _clock.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        _clock.ForeColor = UiTheme.TextMuted;
        _clock.AutoSize = true;
        _clock.TextAlign = ContentAlignment.MiddleRight;
        _clock.BackColor = UiTheme.CardWhite;

        var profile = BuildProfileChip();
        profile.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        header.Layout += (_, _) =>
        {
            _clock.Left = Math.Max(320, header.ClientSize.Width - profile.Width - _clock.Width - 48);
            _clock.Top = (header.ClientSize.Height - _clock.Height) / 2;
            profile.Left = header.ClientSize.Width - profile.Width - 28;
            profile.Top = (header.ClientSize.Height - profile.Height) / 2;
        };

        header.Controls.Add(_headerTitle);
        header.Controls.Add(_headerSubtitle);
        header.Controls.Add(_clock);
        header.Controls.Add(profile);

        _workspaceCard.Paint += (_, e) => UiChrome.PaintCardWithShadow(_workspaceCard, e, UiTheme.CardCornerRadius);
        _workspace.Controls.Add(_workspaceCard);

        col.Controls.Add(header, 0, 0);
        col.Controls.Add(_workspace, 0, 1);
        return col;
    }

    private void UpdateClock() =>
        _clock.Text = $"{DateTime.Now:MMMM d, yyyy}   |   {DateTime.Now:h:mm tt}";

    private Panel BuildProfileChip()
    {
        var wrap = new Panel
        {
            Width = 240,
            Height = 56,
            BackColor = Color.Transparent
        };

        var avatar = new Panel
        {
            Size = new Size(44, 44),
            Location = new Point(0, 6),
            BackColor = UiTheme.AccentMintSoft
        };
        avatar.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var letter = string.IsNullOrWhiteSpace(_employee.Username)
                ? (_employee.FullName.Length > 0 ? _employee.FullName[0].ToString() : "?")
                : _employee.Username.Trim()[0].ToString().ToUpperInvariant();
            using var path = UiChrome.CreateRoundRectPath(new Rectangle(0, 0, avatar.Width - 1, avatar.Height - 1), 22);
            using (var b = new SolidBrush(UiTheme.PrimaryButton))
                e.Graphics.FillPath(b, path);
            TextRenderer.DrawText(
                e.Graphics,
                letter,
                new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
                avatar.ClientRectangle,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        };

        var roleText = EmployeeRoleNames.TryParse(_employee.Role, out var pr)
            ? (pr == EmployeeRole.Admin ? "Administrator" : EmployeeRoleNames.ToStoredString(pr))
            : _employee.Role;

        var name = new Label
        {
            Text = string.IsNullOrWhiteSpace(_employee.Username) ? _employee.FullName : _employee.Username,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Location = new Point(52, 6),
            MaximumSize = new Size(170, 0)
        };
        var role = new Label
        {
            Text = roleText,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(52, 30)
        };

        var menuBtn = new Button
        {
            Text = "▾",
            Size = new Size(28, 28),
            Location = new Point(208, 14),
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

        wrap.Controls.Add(avatar);
        wrap.Controls.Add(name);
        wrap.Controls.Add(role);
        wrap.Controls.Add(menuBtn);
        return wrap;
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
        switch (kind)
        {
            case ClinicShellNavKind.Dashboard:
                if (_homePanel is null)
                {
                    CustomerDirectoryService? cust = ShellNavPermissions.CanAccess(_employee, ClinicShellNavKind.Customers)
                        ? _customerDirectory
                        : null;
                    _homePanel = new StaffHomeDashboardPanel(_employee, cust, Navigate);
                }

                return _homePanel;

            case ClinicShellNavKind.UsersEmployees:
            case ClinicShellNavKind.PendingApprovals:
                if (_usersHub is null)
                {
                    _usersHub = new AdminUsersManagementPanel(_employee, _repository, _registration, _approvals);
                    _usersHub.StaffDirectoryChanged += async (_, _) => await RefreshPendingBadgeAsync();
                }

                var tab = kind == ClinicShellNavKind.PendingApprovals ? UsersHubTab.Pending : UsersHubTab.All;
                _usersHub.SetHubTab(tab);
                return _usersHub;

            case ClinicShellNavKind.Customers:
                return GetLazy(kind, new CustomersHubPanel(_customerDirectory, _employee));

            case ClinicShellNavKind.Animals:
                return GetLazy(kind, new CustomerAnimalsPanel(_customerDirectory));

            case ClinicShellNavKind.Visits:
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
