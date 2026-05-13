using System.Drawing.Drawing2D;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class DashboardPage : UserControl
{
    private static readonly DashboardSection[] NavOrder =
    [
        DashboardSection.Home,
        DashboardSection.Visits,
        DashboardSection.Patients,
        DashboardSection.Billing,
        DashboardSection.CustomerRegistration,
        DashboardSection.CustomerSearch,
        DashboardSection.CustomerAnimals,
        DashboardSection.Staff,
        DashboardSection.PendingEmployees
    ];

    private readonly Employee _employee;
    private readonly MainShellForm _shell;
    private readonly EmployeeRegistrationService _registration;
    private readonly EmployeeApprovalService _approvals;
    private readonly IEmployeeRepository _repository;
    private readonly CustomerDirectoryService _customerDirectory;
    private readonly Panel _sidebar = new();
    private readonly Panel _contentHost = new();
    private readonly Panel _card = new();
    private readonly Panel _viewHost = new();
    private readonly Dictionary<DashboardSection, SidebarNavItem> _navPanels = new();
    private readonly Dictionary<DashboardSection, Control> _viewCache = new();
    private DashboardSection _activeSection = DashboardSection.Home;

    public DashboardPage(
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
        BackColor = UiTheme.PageBackground;
        Font = shell.Font;

        var roleLabel = EmployeeRoleNames.TryParse(_employee.Role, out var parsed)
            ? EmployeeRoleNames.ToStoredString(parsed)
            : _employee.Role;

        var header = UiHeaderBar.Create($"Signed in as {_employee.FullName} — {roleLabel}");

        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.PageBackground
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, UiTheme.SidebarWidth));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        split.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _sidebar.Dock = DockStyle.Fill;
        _sidebar.BackColor = UiTheme.SidebarBackground;
        _sidebar.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.SidebarBorder, 1);
            e.Graphics.DrawLine(pen, _sidebar.Width - 1, 0, _sidebar.Width - 1, _sidebar.Height);
        };

        var topBrand = new Panel
        {
            Height = 132,
            Dock = DockStyle.Top,
            BackColor = UiTheme.SidebarBackground,
            Padding = new Padding(20, 28, 20, 12)
        };
        topBrand.Controls.Add(new Label
        {
            Text = "ClinicVets",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Location = new Point(20, 28)
        });
        topBrand.Controls.Add(new Label
        {
            Text = _employee.FullName,
            Font = new Font("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(20, 58),
            MaximumSize = new Size(UiTheme.SidebarWidth - 40, 0)
        });
        topBrand.Controls.Add(new Label
        {
            Text = roleLabel,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.PrimaryButton,
            AutoSize = true,
            Location = new Point(20, 88)
        });

        var navFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(12, 8, 12, 8),
            BackColor = UiTheme.SidebarBackground
        };

        foreach (var section in NavOrder)
        {
            if (!RolePermissions.CanAccessDashboardSection(_employee, section))
                continue;

            var nav = MakeNavEntry(section, SectionCaption(section));
            _navPanels[section] = nav;
            navFlow.Controls.Add(nav);
        }

        var bottomBar = new Panel
        {
            Height = 96,
            Dock = DockStyle.Bottom,
            BackColor = UiTheme.SidebarBackground,
            Padding = new Padding(16, 12, 16, 20)
        };
        var signOut = new ModernOutlineButton
        {
            Text = "Sign out",
            Dock = DockStyle.Top,
            Height = UiTheme.SecondaryButtonHeight
        };
        signOut.Click += (_, _) => _shell.NavigateToLogin();
        bottomBar.Controls.Add(signOut);

        _sidebar.Controls.Add(navFlow);
        _sidebar.Controls.Add(bottomBar);
        _sidebar.Controls.Add(topBrand);

        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = UiTheme.ContentCanvas;
        _contentHost.Padding = new Padding(28, 24, 28, 28);

        _card.BackColor = UiTheme.ContentCanvas;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        _viewHost.Dock = DockStyle.Fill;
        _viewHost.BackColor = UiTheme.CardWhite;
        _viewHost.Padding = new Padding(40, 36, 40, 32);
        _card.Controls.Add(_viewHost);

        split.Controls.Add(_sidebar, 0, 0);
        split.Controls.Add(_contentHost, 1, 0);
        _contentHost.Controls.Add(_card);

        var pageRoot = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.PageBackground };
        pageRoot.Paint += PaintBodyGradient;
        pageRoot.Controls.Add(split);
        pageRoot.Controls.Add(header);

        Controls.Add(pageRoot);

        Resize += (_, _) => Relayout();
        Load += (_, _) =>
        {
            Relayout();
            SelectSection(DashboardSection.Home);
        };
    }

    private SidebarNavItem MakeNavEntry(DashboardSection section, string caption)
    {
        var nav = new SidebarNavItem(section, caption)
        {
            Width = UiTheme.SidebarWidth - 36,
            Margin = new Padding(8, 4, 8, 4)
        };
        nav.Click += (_, _) => SelectSection(section);
        return nav;
    }

    private void SelectSection(DashboardSection section)
    {
        if (!RolePermissions.CanAccessDashboardSection(_employee, section))
            return;

        _activeSection = section;

        foreach (var kv in _navPanels)
            kv.Value.IsActive = kv.Key == section;

        while (_viewHost.Controls.Count > 0)
        {
            var old = _viewHost.Controls[0];
            _viewHost.Controls.Remove(old);
        }

        var next = GetOrCreateView(section);
        next.Dock = DockStyle.Fill;
        _viewHost.Controls.Add(next);
    }

    private Control GetOrCreateView(DashboardSection section)
    {
        if (_viewCache.TryGetValue(section, out var cached))
            return cached;

        Control created = section switch
        {
            DashboardSection.Home => BuildHomeView(),
            DashboardSection.Staff => new EmployeeStaffPanel(_employee, _repository, _registration),
            DashboardSection.PendingEmployees => new PendingEmployeesPanel(_employee, _approvals),
            DashboardSection.CustomerRegistration => new CustomerRegistrationPanel(_customerDirectory),
            DashboardSection.CustomerSearch => new CustomerSearchPanel(_customerDirectory),
            DashboardSection.CustomerAnimals => new CustomerAnimalsPanel(_customerDirectory),
            _ => BuildPlaceholderView(section)
        };

        _viewCache[section] = created;
        return created;
    }

    private Control BuildHomeView()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.CardWhite
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 216F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var nameParts = _employee.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var shortName = nameParts.Length > 0 ? nameParts[0] : "there";
        var loginLine = string.IsNullOrWhiteSpace(_employee.Username)
            ? $"{_employee.Email}   ·   {_employee.Role}"
            : $"{_employee.Username}   ·   {_employee.Email}   ·   {_employee.Role}";

        var welcomeHeading = new Label
        {
            Text = $"Hello, {shortName}",
            Font = new Font("Segoe UI", 26F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = false,
            Height = 48,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 6)
        };
        var metaLine = new Label
        {
            Text = loginLine,
            ForeColor = UiTheme.TextMuted,
            AutoSize = false,
            Height = 32,
            TextAlign = ContentAlignment.MiddleLeft,
            Font = UiStyles.HeroSubtitleFont,
            Margin = new Padding(0, 0, 0, 20)
        };

        var introStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8),
            BackColor = UiTheme.CardWhite
        };
        introStack.Controls.Add(welcomeHeading);
        introStack.Controls.Add(metaLine);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 16),
            BackColor = UiTheme.CardWhite
        };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var m0 = MakeMetricTile("Session", "Active", UiTheme.MetricAccentStripe, new Padding(0, 0, 12, 0));
        var m1 = MakeMetricTile("Workspace", "ClinicVets", UiTheme.PrimaryButton, new Padding(6, 0, 6, 0));
        var m2 = MakeMetricTile("Build", "Course demo", UiTheme.TextMuted, new Padding(12, 0, 0, 0));
        metrics.Controls.Add(m0, 0, 0);
        metrics.Controls.Add(m1, 1, 0);
        metrics.Controls.Add(m2, 2, 0);
        m0.Dock = DockStyle.Fill;
        m1.Dock = DockStyle.Fill;
        m2.Dock = DockStyle.Fill;

        var detailInfo = new Label
        {
            Text =
                "This dashboard uses a single-window layout with a role-aware sidebar. " +
                "Administrators see every module; other roles only see the areas that match their responsibilities.",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 0)
        };

        root.Controls.Add(introStack, 0, 0);
        root.Controls.Add(metrics, 0, 1);
        root.Controls.Add(detailInfo, 0, 2);

        return root;
    }

    private static Control BuildPlaceholderView(DashboardSection section)
    {
        var title = SectionLongTitle(section);
        var body = SectionPlaceholderBody(section);

        var stack = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(4),
            BackColor = UiTheme.CardWhite
        };

        var heading = UiStyles.CreateHeroTitle(title);
        var subtitle = UiStyles.CreateHeroSubtitle(body);
        stack.Controls.Add(heading);
        stack.Controls.Add(subtitle);

        return stack;
    }

    private static string SectionCaption(DashboardSection section) =>
        section switch
        {
            DashboardSection.Home => "Home",
            DashboardSection.Visits => "Visits",
            DashboardSection.Patients => "Patients",
            DashboardSection.Billing => "Billing",
            DashboardSection.CustomerRegistration => "New customer",
            DashboardSection.CustomerSearch => "Find customer",
            DashboardSection.CustomerAnimals => "Pet records",
            DashboardSection.Staff => "Staff",
            DashboardSection.PendingEmployees => "Pending Employees",
            _ => "Home"
        };

    private static string SectionLongTitle(DashboardSection section) =>
        section switch
        {
            DashboardSection.Visits => "Visits workspace",
            DashboardSection.Patients => "Patients workspace",
            DashboardSection.Billing => "Billing workspace",
            _ => "Workspace"
        };

    private static string SectionPlaceholderBody(DashboardSection section) =>
        section switch
        {
            DashboardSection.Visits =>
                "Scheduling and visit notes will live here. This desktop build keeps the area as a focused placeholder for the course demo.",
            DashboardSection.Patients =>
                "Medical records and patient profiles will live here. Veterinarians (and administrators) can open this area once the module is implemented.",
            DashboardSection.Billing =>
                "Invoices and payments will live here. Secretaries (and administrators) can open this area once the module is implemented.",
            _ => "This section is reserved for future functionality."
        };

    private static Panel MakeMetricTile(string title, string value, Color accent, Padding margin)
    {
        var p = new Panel
        {
            Margin = margin,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(22, 20, 20, 20)
        };
        p.Paint += (_, e) => UiChrome.PaintMetricTile(p, e, accent);

        var t = new Label
        {
            Text = title,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(22, 18),
            BackColor = UiTheme.MetricTileBackground
        };
        var v = new Label
        {
            Text = value,
            ForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(22, 44),
            BackColor = UiTheme.MetricTileBackground
        };
        p.Controls.Add(t);
        p.Controls.Add(v);
        return p;
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

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_contentHost, _card, 8, 1040, 20, 24);
        if (_card.ClientSize.Width > 0 && _viewHost.Controls.Count > 0)
        {
            var inner = Math.Max(320, _card.ClientSize.Width - 80);
            foreach (Control child in _viewHost.Controls)
                ApplyInnerWidth(child, inner);
        }
    }

    private static void ApplyInnerWidth(Control view, int inner)
    {
        switch (view)
        {
            case TableLayoutPanel table:
            {
                foreach (Control c in table.Controls)
                {
                    if (c is Label lbl && lbl.Dock != DockStyle.Fill)
                    {
                        lbl.Width = inner;
                        lbl.MaximumSize = new Size(inner, 0);
                    }

                    if (c is FlowLayoutPanel flow)
                    {
                        foreach (Control innerCtrl in flow.Controls)
                        {
                            if (innerCtrl is Label l)
                            {
                                l.MaximumSize = new Size(inner, 0);
                                l.Width = inner;
                            }
                        }
                    }
                }

                break;
            }
            case FlowLayoutPanel placeholderFlow:
            {
                foreach (Control c in placeholderFlow.Controls)
                {
                    c.MaximumSize = new Size(inner, 0);
                    c.Width = inner;
                }

                break;
            }
            default:
            {
                if (view is DataGridView)
                    break;
                foreach (Control ch in view.Controls)
                    ApplyInnerWidth(ch, inner);
                break;
            }
        }
    }
}
