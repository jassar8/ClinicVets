using ClinicVets.Core.Entities;
using ClinicVets.Desktop;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Post-login dashboard — full screen with responsive metrics row.
/// </summary>
public class DashboardForm : Form
{
    private readonly Employee _employee;
    private readonly Panel _body = new();
    private readonly Panel _card = new();
    private readonly Label _detailInfo;
    private readonly Label _welcomeHeading;
    private readonly Label _metaLine;

    public DashboardForm(Employee employee)
    {
        _employee = employee;

        Text = "ClinicVets — Dashboard";
        Icon = AppBranding.CreateWindowIcon();
        MinimumSize = new Size(960, 640);
        MaximizeBox = true;
        MinimizeBox = true;
        BackColor = UiTheme.PageBackground;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
        StartPosition = FormStartPosition.Manual;
        Bounds = Screen.PrimaryScreen?.WorkingArea ?? new Rectangle(0, 0, 1280, 800);
        WindowState = FormWindowState.Maximized;

        var header = new Panel
        {
            Height = 78,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderBlue
        };
        header.Controls.Add(new Label
        {
            Text = "ClinicVets",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(40, 14)
        });
        header.Controls.Add(new Label
        {
            Text = "Dashboard — signed-in workspace",
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            Location = new Point(40, 46),
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point)
        });

        _body.Dock = DockStyle.Fill;
        _body.BackColor = UiTheme.PageBackground;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = Color.Transparent;
        _card.Paint += (_, e) => UiChrome.PaintCardWithShadow(_card, e, UiTheme.CardCornerRadius);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(44, 40, 44, 36),
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _welcomeHeading = new Label
        {
            Text = $"Welcome, {_employee.FullName}",
            Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = false,
            Height = 56,
            TextAlign = ContentAlignment.MiddleCenter,
            Margin = new Padding(0, 0, 0, 4)
        };
        _metaLine = new Label
        {
            Text = $"{_employee.Email}   ·   {_employee.Role}",
            ForeColor = UiTheme.TextMuted,
            AutoSize = false,
            Height = 32,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = UiStyles.HeroSubtitleFont,
            Margin = new Padding(0, 0, 0, 20)
        };

        var headerStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8),
            BackColor = Color.Transparent
        };
        headerStack.Controls.Add(_welcomeHeading);
        headerStack.Controls.Add(_metaLine);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 8, 0, 16)
        };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var m0 = MakeMetricTile("Authentication", "Active session", Color.FromArgb(46, 160, 100), new Padding(0, 0, 12, 0));
        var m1 = MakeMetricTile("Module", "Employee access", UiTheme.HeaderBlue, new Padding(6, 0, 6, 0));
        var m2 = MakeMetricTile("Status", "Demo ready", UiTheme.TextDark, new Padding(12, 0, 0, 0));
        metrics.Controls.Add(m0, 0, 0);
        metrics.Controls.Add(m1, 1, 0);
        metrics.Controls.Add(m2, 2, 0);
        m0.Dock = DockStyle.Fill;
        m1.Dock = DockStyle.Fill;
        m2.Dock = DockStyle.Fill;

        _detailInfo = new Label
        {
            Text =
                "This screen is the application home after login. " +
                "You can extend this area with visits, treatments, billing, and other clinic modules as your project grows.",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 20)
        };

        var logout = new Button
        {
            Text = "Sign out",
            Width = 220,
            Font = UiStyles.SecondaryButtonFont,
            Anchor = AnchorStyles.None
        };
        UiStyles.ApplySecondaryButton(logout);
        logout.Height = UiTheme.SecondaryButtonHeight;
        logout.Click += (_, _) => Close();

        var footer = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 8, 0, 0)
        };
        footer.Controls.Add(logout);

        root.Controls.Add(headerStack, 0, 0);
        root.Controls.Add(metrics, 0, 1);
        root.Controls.Add(_detailInfo, 0, 2);
        root.Controls.Add(footer, 0, 3);

        _card.Controls.Add(root);
        _body.Controls.Add(_card);
        Controls.Add(_body);
        Controls.Add(header);

        Resize += (_, _) => Relayout();
        Shown += (_, _) =>
        {
            WindowState = FormWindowState.Maximized;
            Relayout();
        };
    }

    private static Panel MakeMetricTile(string title, string value, Color accent, Padding margin)
    {
        var p = new Panel
        {
            Margin = margin,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(20, 18, 20, 18)
        };
        p.Paint += (_, e) =>
        {
            using var pen = new Pen(accent, 3);
            e.Graphics.DrawLine(pen, 0, 4, 0, p.Height - 4);
        };

        var t = new Label
        {
            Text = title,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(12, 12)
        };
        var v = new Label
        {
            Text = value,
            ForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(12, 38)
        };
        p.Controls.Add(t);
        p.Controls.Add(v);
        return p;
    }

    private void Relayout()
    {
        ResponsiveLayout.CenterCard(_body, _card, 40, 1200, 40, 48);
        if (_card.ClientSize.Width > 0)
        {
            var inner = Math.Max(320, _card.ClientSize.Width - 88);
            _welcomeHeading.Width = inner;
            _metaLine.Width = inner;
            _detailInfo.MaximumSize = new Size(inner, 0);
        }
    }
}
