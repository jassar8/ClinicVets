using System.Drawing.Drawing2D;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class DashboardPage : UserControl
{
    private readonly Employee _employee;
    private readonly MainShellForm _shell;
    private readonly Panel _sidebar = new();
    private readonly Panel _contentHost = new();
    private readonly Panel _card = new();
    private readonly Label _detailInfo;
    private readonly Label _welcomeHeading;
    private readonly Label _metaLine;

    public DashboardPage(Employee employee, MainShellForm shell)
    {
        _employee = employee;
        _shell = shell;

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
        BackColor = UiTheme.PageBackground;
        Font = shell.Font;

        var header = UiHeaderBar.Create($"Signed in as {_employee.FullName}");

        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Color.Transparent
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
            BackColor = Color.Transparent,
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
            Text = _employee.Role,
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
            BackColor = Color.Transparent
        };
        navFlow.Controls.Add(MakeNavPill("Home", active: true));
        navFlow.Controls.Add(MakeNavGhost("Visits (coming soon)"));
        navFlow.Controls.Add(MakeNavGhost("Patients (coming soon)"));
        navFlow.Controls.Add(MakeNavGhost("Billing (coming soon)"));

        var bottomBar = new Panel
        {
            Height = 96,
            Dock = DockStyle.Bottom,
            BackColor = Color.Transparent,
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

        _card.BackColor = Color.Transparent;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(40, 36, 40, 32),
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 216F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var nameParts = _employee.FullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var shortName = nameParts.Length > 0 ? nameParts[0] : "there";
        _welcomeHeading = new Label
        {
            Text = $"Hello, {shortName}",
            Font = new Font("Segoe UI", 26F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = false,
            Height = 48,
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 0, 6)
        };
        _metaLine = new Label
        {
            Text = $"{_employee.Email}   ·   {_employee.Role}",
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
            BackColor = Color.Transparent
        };
        introStack.Controls.Add(_welcomeHeading);
        introStack.Controls.Add(_metaLine);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 16)
        };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var sage = Color.FromArgb(64, 148, 112);
        var m0 = MakeMetricTile("Session", "Active", sage, new Padding(0, 0, 12, 0));
        var m1 = MakeMetricTile("Workspace", "Employee", UiTheme.PrimaryButton, new Padding(6, 0, 6, 0));
        var m2 = MakeMetricTile("Build", "Course demo", UiTheme.TextMuted, new Padding(12, 0, 0, 0));
        metrics.Controls.Add(m0, 0, 0);
        metrics.Controls.Add(m1, 1, 0);
        metrics.Controls.Add(m2, 2, 0);
        m0.Dock = DockStyle.Fill;
        m1.Dock = DockStyle.Fill;
        m2.Dock = DockStyle.Fill;

        _detailInfo = new Label
        {
            Text =
                "This dashboard uses a single-window layout with a sidebar for future modules " +
                "(visits, patients, billing). Extend the content area while keeping the same modern shell.",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 0)
        };

        root.Controls.Add(introStack, 0, 0);
        root.Controls.Add(metrics, 0, 1);
        root.Controls.Add(_detailInfo, 0, 2);

        _card.Controls.Add(root);
        _contentHost.Controls.Add(_card);

        split.Controls.Add(_sidebar, 0, 0);
        split.Controls.Add(_contentHost, 1, 0);

        var pageRoot = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.PageBackground };
        pageRoot.Paint += PaintBodyGradient;
        pageRoot.Controls.Add(split);
        pageRoot.Controls.Add(header);

        Controls.Add(pageRoot);

        Resize += (_, _) => Relayout();
        Load += (_, _) => Relayout();
    }

    private static Control MakeNavPill(string text, bool active)
    {
        var p = new Panel
        {
            Width = UiTheme.SidebarWidth - 36,
            Height = 46,
            Margin = new Padding(8, 4, 8, 4),
            BackColor = active ? UiTheme.SidebarItemActive : Color.Transparent,
            Cursor = Cursors.Default
        };
        p.Controls.Add(new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.PrimaryButton,
            AutoSize = true,
            Location = new Point(18, 12),
            BackColor = Color.Transparent
        });
        return p;
    }

    private static Control MakeNavGhost(string text)
    {
        var lbl = new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.SidebarMuted,
            AutoSize = false,
            Height = 40,
            Width = UiTheme.SidebarWidth - 36,
            Margin = new Padding(20, 2, 8, 2),
            TextAlign = ContentAlignment.MiddleLeft,
            Cursor = Cursors.Default
        };
        return lbl;
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
            Color.FromArgb(232, 242, 238),
            LinearGradientMode.Vertical);
        g.FillRectangle(brush, c.ClientRectangle);
    }

    private static Panel MakeMetricTile(string title, string value, Color accent, Padding margin)
    {
        var p = new Panel
        {
            Margin = margin,
            BackColor = Color.Transparent,
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
            BackColor = Color.Transparent
        };
        var v = new Label
        {
            Text = value,
            ForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 16F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(22, 44),
            BackColor = Color.Transparent
        };
        p.Controls.Add(t);
        p.Controls.Add(v);
        return p;
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_contentHost, _card, 8, 1040, 20, 24);
        if (_card.ClientSize.Width > 0)
        {
            var inner = Math.Max(320, _card.ClientSize.Width - 80);
            _welcomeHeading.Width = inner;
            _metaLine.Width = inner;
            _detailInfo.MaximumSize = new Size(inner, 0);
        }
    }
}
