using ClinicVets.Core.Entities;
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

    public DashboardForm(Employee employee)
    {
        _employee = employee;

        Text = "ClinicVets — Dashboard";
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
            Height = 100,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderBlue
        };
        header.Controls.Add(new Label
        {
            Text = "ClinicVets Dashboard",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 24F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(40, 22)
        });
        header.Controls.Add(new Label
        {
            Text = "You are signed in to the clinic management demo.",
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            Location = new Point(40, 62),
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point)
        });

        _body.Dock = DockStyle.Fill;
        _body.BackColor = UiTheme.PageBackground;
        _body.Resize += (_, _) => Relayout();

        _card.BackColor = UiTheme.CardWhite;
        _card.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, _card.Width - 1, _card.Height - 1);
        };

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Padding = new Padding(40, 36, 40, 32)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Absolute, 220F));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var welcome = new Label
        {
            Text = $"Welcome, {_employee.FullName}",
            Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8)
        };
        var meta = new Label
        {
            Text = $"{_employee.Email}   ·   {_employee.Role}",
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Font = new Font("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(0, 0, 0, 20)
        };

        var headerStack = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.TopDown,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8)
        };
        headerStack.Controls.Add(welcome);
        headerStack.Controls.Add(meta);

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
            Text = "Logout",
            Height = 52,
            Width = 200,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            Anchor = AnchorStyles.Left
        };
        logout.Click += (_, _) => Close();

        var footer = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false
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
        ResponsiveLayout.CenterCard(_body, _card, 40, 1200, 48, 48);
        if (_card.ClientSize.Width > 0)
            _detailInfo.MaximumSize = new Size(Math.Max(320, _card.ClientSize.Width - 120), 0);
    }
}
