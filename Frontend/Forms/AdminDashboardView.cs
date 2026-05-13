using System.Drawing;
using System.Drawing.Drawing2D;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Full-window administrator experience: dark teal sidebar, light workspace, unified user management.
/// </summary>
public sealed class AdminDashboardView : UserControl
{
    private readonly Employee _employee;
    private readonly MainShellForm _shell;
    private readonly EmployeeRegistrationService _registration;
    private readonly EmployeeApprovalService _approvals;
    private readonly IEmployeeRepository _repository;
    private readonly CustomerDirectoryService _customerDirectory;

    private readonly Dictionary<AdminNavKind, AdminSidebarNavItem> _navItems = new();
    private readonly Dictionary<AdminNavKind, Control> _lazyPages = new();

    private readonly Label _headerTitle = new();
    private readonly Label _headerSubtitle = new();
    private readonly Panel _workspace = new() { Dock = DockStyle.Fill, BackColor = UiTheme.ContentCanvas, Padding = new Padding(24, 20, 24, 24) };
    private readonly Panel _workspaceCard = new() { Dock = DockStyle.Fill, BackColor = UiTheme.CardWhite, Padding = new Padding(4) };

    private AdminUsersManagementPanel? _usersHub;
    private AdminSidebarNavItem? _navPending;

    public AdminDashboardView(
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

        var sidebar = BuildSidebar();
        var main = BuildMainColumn();

        split.Controls.Add(sidebar, 0, 0);
        split.Controls.Add(main, 1, 0);

        Controls.Add(split);

        Load += async (_, _) =>
        {
            Navigate(AdminNavKind.Dashboard);
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

        void AddGroup(string text)
        {
            var g = new Label
            {
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point),
                ForeColor = Color.FromArgb(160, 200, 194),
                AutoSize = true,
                Margin = new Padding(10, 14, 8, 6),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = UiTheme.AdminSidebarBackground
            };
            navHost.Controls.Add(g);
        }

        AdminSidebarNavItem AddNav(string caption, AdminNavKind kind)
        {
            var row = new AdminSidebarNavItem(caption, kind)
            {
                Width = 248,
                Margin = new Padding(6, 2, 6, 2)
            };
            row.Click += (_, _) => Navigate(kind);
            _navItems[kind] = row;
            navHost.Controls.Add(row);
            return row;
        }

        AddNav("Dashboard", AdminNavKind.Dashboard);
        AddGroup("MANAGEMENT");
        AddNav("Users & employees", AdminNavKind.UsersEmployees);
        _navPending = AddNav("Pending approvals", AdminNavKind.PendingApprovals);
        AddNav("Roles & permissions", AdminNavKind.RolesPermissions);
        AddGroup("CLINIC");
        AddNav("Customers", AdminNavKind.Customers);
        AddNav("Animals", AdminNavKind.Animals);
        AddNav("Visits", AdminNavKind.Visits);
        AddNav("Treatments", AdminNavKind.Treatments);
        AddGroup("SYSTEM");
        AddNav("Settings", AdminNavKind.Settings);

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
        col.RowStyles.Add(new RowStyle(SizeType.Absolute, 96F));
        col.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var header = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.CardWhite,
            Padding = new Padding(28, 18, 28, 14)
        };
        header.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, header.Height - 1, header.Width, header.Height - 1);
        };

        _headerTitle.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
        _headerTitle.ForeColor = UiTheme.TextDark;
        _headerTitle.AutoSize = true;
        _headerTitle.Location = new Point(28, 16);

        _headerSubtitle.Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        _headerSubtitle.ForeColor = UiTheme.TextMuted;
        _headerSubtitle.AutoSize = true;
        _headerSubtitle.Location = new Point(28, 52);
        _headerSubtitle.MaximumSize = new Size(700, 0);

        var profile = BuildProfileChip();
        profile.Anchor = AnchorStyles.Top | AnchorStyles.Right;

        header.Layout += (_, _) =>
        {
            profile.Left = header.ClientSize.Width - profile.Width - 28;
            profile.Top = (header.ClientSize.Height - profile.Height) / 2;
        };

        header.Controls.Add(_headerTitle);
        header.Controls.Add(_headerSubtitle);
        header.Controls.Add(profile);

        _workspaceCard.Paint += (_, e) => UiChrome.PaintCardWithShadow(_workspaceCard, e, UiTheme.CardCornerRadius);
        _workspace.Controls.Add(_workspaceCard);

        col.Controls.Add(header, 0, 0);
        col.Controls.Add(_workspace, 0, 1);
        return col;
    }

    private Panel BuildProfileChip()
    {
        var wrap = new Panel
        {
            Width = 220,
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

        var name = new Label
        {
            Text = string.IsNullOrWhiteSpace(_employee.Username) ? _employee.FullName : _employee.Username,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Location = new Point(52, 8),
            MaximumSize = new Size(150, 0)
        };
        var role = new Label
        {
            Text = "Administrator",
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(52, 30)
        };

        var menuBtn = new Button
        {
            Text = "▾",
            Size = new Size(28, 28),
            Location = new Point(188, 14),
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

    private void Navigate(AdminNavKind kind)
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
    }

    private Control ResolvePage(AdminNavKind kind)
    {
        switch (kind)
        {
            case AdminNavKind.Dashboard:
            case AdminNavKind.UsersEmployees:
            case AdminNavKind.PendingApprovals:
                if (_usersHub is null)
                {
                    _usersHub = new AdminUsersManagementPanel(_employee, _repository, _registration, _approvals);
                    _usersHub.StaffDirectoryChanged += async (_, _) => await RefreshPendingBadgeAsync();
                }

                var tab = kind switch
                {
                    AdminNavKind.PendingApprovals => UsersHubTab.Pending,
                    AdminNavKind.UsersEmployees => UsersHubTab.All,
                    _ => UsersHubTab.All
                };
                _usersHub.SetHubTab(tab);
                return _usersHub;

            case AdminNavKind.RolesPermissions:
                return GetLazy(
                    kind,
                    new AdminPlaceholderPanel(
                        "Roles & permissions",
                        "Define role templates and access policies. This module is reserved for a future course milestone."));

            case AdminNavKind.Customers:
                return GetLazy(kind, new CustomerSearchPanel(_customerDirectory));

            case AdminNavKind.Animals:
                return GetLazy(kind, new CustomerAnimalsPanel(_customerDirectory));

            case AdminNavKind.Visits:
                return GetLazy(
                    kind,
                    new AdminPlaceholderPanel(
                        "Visits",
                        "Scheduling, check-in, and visit documentation will be added here in a future iteration."));

            case AdminNavKind.Treatments:
                return GetLazy(
                    kind,
                    new AdminPlaceholderPanel(
                        "Treatments",
                        "Treatment plans and protocols will be managed from this workspace once implemented."));

            case AdminNavKind.Settings:
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

    private Control GetLazy(AdminNavKind kind, Control created)
    {
        if (_lazyPages.TryGetValue(kind, out var existing))
            return existing;
        _lazyPages[kind] = created;
        return created;
    }

    private void UpdateHeaderCopy(AdminNavKind kind)
    {
        var (title, sub) = kind switch
        {
            AdminNavKind.Dashboard => ("Admin Dashboard", "Manage users, employees and system settings"),
            AdminNavKind.UsersEmployees => ("Users & employees", "Directory, filters, and account actions"),
            AdminNavKind.PendingApprovals => ("Pending approvals", "Assign Employee IDs and finalize roles before employees can sign in"),
            AdminNavKind.RolesPermissions => ("Roles & permissions", "Control who can access each part of the clinic system"),
            AdminNavKind.Customers => ("Customers", "Search and review registered customers"),
            AdminNavKind.Animals => ("Animals", "Household pets linked to customer records"),
            AdminNavKind.Visits => ("Visits", "Clinical visits and scheduling"),
            AdminNavKind.Treatments => ("Treatments", "Treatment history and protocols"),
            AdminNavKind.Settings => ("Settings", "Clinic configuration"),
            _ => ("Admin Dashboard", "Manage users, employees and system settings")
        };

        _headerTitle.Text = title;
        _headerSubtitle.Text = sub;
    }

    private async Task RefreshPendingBadgeAsync()
    {
        try
        {
            var pending = await _repository.GetPendingRegistrationsAsync();
            if (_navPending is null)
                return;
            _navPending.BadgeCount = pending.Count > 0 ? pending.Count : null;
            _navPending.Invalidate();
        }
        catch
        {
            // demo store — ignore badge refresh failures
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
}
