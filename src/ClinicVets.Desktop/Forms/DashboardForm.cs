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

        var header = UiHeaderBar.Create("Home — your clinic workspace overview");

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
            Padding = new Padding(48, 44, 48, 40),
            BackColor = Color.Transparent
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 224F));
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
            Margin = new Padding(0, 4, 0, 4)
        };
        _metaLine = new Label
        {
            Text = $"{_employee.Email}   ·   {_employee.Role}",
            ForeColor = UiTheme.TextMuted,
            AutoSize = false,
            Height = 34,
            TextAlign = ContentAlignment.MiddleCenter,
            Font = UiStyles.HeroSubtitleFont,
            Margin = new Padding(0, 0, 0, 22)
        };

        var headerStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 10),
            BackColor = Color.Transparent
        };
        headerStack.Controls.Add(_welcomeHeading);
        headerStack.Controls.Add(_metaLine);

        var metrics = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 4, 0, 18)
        };
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        metrics.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        metrics.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        var sage = Color.FromArgb(74, 138, 108);
        var m0 = MakeMetricTile("Session", "Signed in", sage, new Padding(0, 0, 12, 0));
        var m1 = MakeMetricTile("Access", "Employee portal", UiTheme.PrimaryButton, new Padding(6, 0, 6, 0));
        var m2 = MakeMetricTile("Environment", "Demo build", UiTheme.TextMuted, new Padding(12, 0, 0, 0));
        metrics.Controls.Add(m0, 0, 0);
        metrics.Controls.Add(m1, 1, 0);
        metrics.Controls.Add(m2, 2, 0);
        m0.Dock = DockStyle.Fill;
        m1.Dock = DockStyle.Fill;
        m2.Dock = DockStyle.Fill;

        _detailInfo = new Label
        {
            Text =
                "This home screen is ready for course demos. From here you can later add visits, " +
                "treatments, billing, and patient records while keeping the same calm clinic styling.",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 10, 0, 22)
        };

        var logout = new Button
        {
            Text = "Sign out",
            Width = 240,
            Font = UiStyles.SecondaryButtonFont,
            Anchor = AnchorStyles.None
        };
        UiStyles.ApplySecondaryButton(logout);
        logout.Height = UiTheme.SecondaryButtonHeight;
        logout.Margin = new Padding(0, 4, 0, 0);
        logout.Click += (_, _) => Close();

        var footer = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0)
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
            BackColor = Color.Transparent,
            Padding = new Padding(22, 20, 20, 20)
        };
        p.Paint += (_, e) => UiChrome.PaintMetricTile(p, e, accent);

        var t = new Label
        {
            Text = title,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 10.25F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(22, 18),
            BackColor = Color.Transparent
        };
        var v = new Label
        {
            Text = value,
            ForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 15.5F, FontStyle.Bold, GraphicsUnit.Point),
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
        ResponsiveLayout.CenterCard(_body, _card, 48, 1180, 44, 52);
        if (_card.ClientSize.Width > 0)
        {
            var inner = Math.Max(320, _card.ClientSize.Width - 96);
            _welcomeHeading.Width = inner;
            _metaLine.Width = inner;
            _detailInfo.MaximumSize = new Size(inner, 0);
        }
    }
}
