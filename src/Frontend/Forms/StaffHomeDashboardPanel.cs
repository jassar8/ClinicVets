using System.Drawing;
using System.Drawing.Drawing2D;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>Role-aware home dashboard with metrics, schedule preview, activity feed, and quick actions.</summary>
public sealed class StaffHomeDashboardPanel : UserControl
{
    private readonly Employee _employee;
    private readonly CustomerDirectoryService? _customers;
    private readonly Action<ClinicShellNavKind> _navigate;

    private readonly Label _metricCustomers = new();
    private readonly Label _metricAnimals = new();

    public StaffHomeDashboardPanel(Employee employee, CustomerDirectoryService? customers, Action<ClinicShellNavKind> navigate)
    {
        _employee = employee;
        _customers = customers;
        _navigate = navigate;

        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.CardWhite
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(BuildMetricsRow(), 0, 0);
        root.Controls.Add(BuildBody(), 0, 1);
        root.Controls.Add(BuildFooter(), 0, 2);

        Controls.Add(root);

        Load += async (_, _) => await RefreshMetricsAsync();
    }

    public async Task RefreshMetricsAsync()
    {
        if (_customers is null ||
            (!RolePermissions.CanAccessDashboardSection(_employee, DashboardSection.CustomerSearch) &&
             !RolePermissions.CanAccessDashboardSection(_employee, DashboardSection.CustomerRegistration)))
        {
            _metricCustomers.Text = "—";
            _metricAnimals.Text = "—";
            return;
        }

        try
        {
            var list = await _customers.ListCustomersAsync();
            _metricCustomers.Text = list.Count.ToString("D0");
            var animalCount = 0;
            foreach (var c in list)
            {
                var animals = await _customers.GetAnimalsForCustomerAsync(c.Id);
                animalCount += animals.Count;
            }

            _metricAnimals.Text = animalCount.ToString("D0");
        }
        catch
        {
            _metricCustomers.Text = "—";
            _metricAnimals.Text = "—";
        }
    }

    private Panel BuildMetricsRow()
    {
        var wrap = new Panel { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 0, 0, 16), BackColor = UiTheme.CardWhite };
        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 124,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = UiTheme.CardWhite
        };
        for (var i = 0; i < 4; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        grid.Controls.Add(HomeMetricCard("Today's appointments", "8", UiTheme.PrimaryButton, null), 0, 0);
        grid.Controls.Add(HomeMetricCard("Active animals", "—", UiTheme.MetricAccentPending, _metricAnimals), 1, 0);
        grid.Controls.Add(HomeMetricCard("Registered customers", "—", UiTheme.AccentStrip, _metricCustomers), 2, 0);
        grid.Controls.Add(HomeMetricCard("Treatments given", "23", UiTheme.MetricAccentSuccess, null), 3, 0);

        wrap.Controls.Add(grid);
        return wrap;
    }

    private static Panel HomeMetricCard(string title, string initialValue, Color accent, Label? bindValue)
    {
        var p = new Panel
        {
            Margin = new Padding(0, 0, 12, 0),
            Dock = DockStyle.Fill,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(18, 14, 14, 10)
        };
        p.Paint += (_, e) => UiChrome.PaintMetricTile(p, e, accent);

        var icon = new Panel
        {
            Size = new Size(36, 36),
            Location = new Point(18, 18),
            BackColor = UiTheme.MetricTileBackground
        };
        icon.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(Color.FromArgb(210, accent));
            e.Graphics.FillEllipse(b, 2, 2, icon.Width - 5, icon.Height - 5);
        };

        var t = new Label
        {
            Text = title,
            Location = new Point(62, 16),
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            BackColor = UiTheme.MetricTileBackground
        };

        var value = bindValue ?? new Label();
        value.Text = initialValue;
        value.Location = new Point(62, 36);
        value.Size = new Size(140, 40);
        value.TextAlign = ContentAlignment.MiddleLeft;
        value.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
        value.ForeColor = UiTheme.TextDark;
        value.BackColor = UiTheme.MetricTileBackground;

        var note = new Label
        {
            Text = "Demo figures where noted · Live counts for customers and animals",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(18, 96),
            MaximumSize = new Size(220, 0),
            BackColor = UiTheme.MetricTileBackground
        };

        p.Controls.Add(icon);
        p.Controls.Add(t);
        p.Controls.Add(value);
        p.Controls.Add(note);
        return p;
    }

    private Panel BuildBody()
    {
        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.CardWhite
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));

        split.Controls.Add(BuildScheduleCard(), 0, 0);
        split.Controls.Add(BuildRightColumn(), 1, 0);
        return split;
    }

    private Panel BuildScheduleCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 14, 0),
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(20, 18, 20, 16)
        };
        card.Paint += (_, e) => UiChrome.PaintMetricTile(card, e, UiTheme.MetricAccentStripe);

        var head = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 44,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = UiTheme.MetricTileBackground
        };
        head.Controls.Add(new Label
        {
            Text = "Today's schedule",
            Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Margin = new Padding(0, 6, 16, 0),
            BackColor = UiTheme.MetricTileBackground
        });
        var cal = new ModernOutlineButton { Text = "View calendar", AutoSize = true, Height = 36, Margin = new Padding(0, 2, 0, 0) };
        cal.Click += (_, _) => _navigate(ClinicShellNavKind.Visits);
        head.Controls.Add(cal);

        var rows = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 8, 0, 8),
            BackColor = UiTheme.MetricTileBackground
        };
        rows.Controls.Add(MakeScheduleRow("10:00 AM", "Buddy", "Golden Retriever", "Vaccination", "Scheduled"));
        rows.Controls.Add(MakeScheduleRow("11:30 AM", "Luna", "Siamese cat", "Check-up", "Completed"));
        rows.Controls.Add(MakeScheduleRow("02:15 PM", "Rocky", "German Shepherd", "Follow-up", "Scheduled"));

        rows.SizeChanged += (_, _) =>
        {
            var w = rows.ClientSize.Width - rows.Padding.Horizontal;
            foreach (Control c in rows.Controls)
                c.Width = Math.Max(200, w);
        };

        var newVisit = new ModernPrimaryButton
        {
            Text = "+  New visit",
            Dock = DockStyle.Bottom,
            Height = 48,
            Margin = new Padding(0, 12, 0, 0)
        };
        newVisit.Click += (_, _) => _navigate(ClinicShellNavKind.Visits);

        card.Controls.Add(head);
        card.Controls.Add(rows);
        card.Controls.Add(newVisit);
        return card;
    }

    private static Panel MakeScheduleRow(string time, string pet, string breed, string service, string status)
    {
        var row = new Panel
        {
            Height = 64,
            Margin = new Padding(0, 0, 0, 10),
            BackColor = UiTheme.CardWhite
        };
        row.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
        };
        row.Resize += (_, _) =>
        {
            foreach (Control c in row.Controls)
            {
                if (c is Label { Name: "pill" })
                    c.Left = row.ClientSize.Width - c.Width - 16;
            }
        };

        var timeBox = new Label
        {
            Text = time,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = UiTheme.PrimaryButton,
            Bounds = new Rectangle(12, 14, 88, 36)
        };

        row.Controls.Add(timeBox);
        row.Controls.Add(new Label
        {
            Text = $"{pet}   ·   {breed}",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Location = new Point(116, 12),
            BackColor = UiTheme.CardWhite
        });
        row.Controls.Add(new Label
        {
            Text = service,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(116, 34),
            BackColor = UiTheme.CardWhite
        });

        var pending = status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase);
        var pill = new Label
        {
            Name = "pill",
            Text = status,
            AutoSize = true,
            Padding = new Padding(10, 4, 10, 4),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = pending ? UiTheme.TealMain : UiTheme.SuccessText,
            BackColor = pending ? UiTheme.AccentMintWash : UiTheme.SuccessBackground
        };
        pill.Location = new Point(400, 18);
        row.Controls.Add(pill);

        row.Dock = DockStyle.Top;
        return row;
    }

    private Panel BuildRightColumn()
    {
        var col = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            RowCount = 2,
            ColumnCount = 1,
            BackColor = UiTheme.CardWhite
        };
        col.RowStyles.Add(new RowStyle(SizeType.Percent, 58F));
        col.RowStyles.Add(new RowStyle(SizeType.Percent, 42F));
        col.Controls.Add(BuildActivitiesCard(), 0, 0);
        col.Controls.Add(BuildQuickActions(), 0, 1);
        return col;
    }

    private Panel BuildActivitiesCard()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, 12),
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(16, 14, 16, 12)
        };
        card.Paint += (_, e) => UiChrome.PaintMetricTile(card, e, UiTheme.AccentStrip);

        var title = new Label
        {
            Text = "Recent activities",
            Dock = DockStyle.Top,
            Height = 32,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = UiTheme.MetricTileBackground
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 6, 0, 0),
            BackColor = UiTheme.MetricTileBackground
        };
        flow.Controls.Add(MakeActivity("●  New customer registered: John Smith", "10 minutes ago"));
        flow.Controls.Add(MakeActivity("●  Visit completed for Buddy", "32 minutes ago"));
        flow.Controls.Add(MakeActivity("●  Treatment logged for Luna", "1 hour ago"));

        var all = new ModernOutlineButton { Text = "View all", Dock = DockStyle.Bottom, Height = 36, Margin = new Padding(0, 8, 0, 0) };
        all.Click += (_, _) => _navigate(ClinicShellNavKind.Customers);

        card.Controls.Add(flow);
        card.Controls.Add(all);
        card.Controls.Add(title);
        return card;
    }

    private static Label MakeActivity(string text, string when)
    {
        return new Label
        {
            Text = text + Environment.NewLine + when,
            AutoSize = true,
            MaximumSize = new Size(320, 0),
            Margin = new Padding(0, 6, 0, 0),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            BackColor = UiTheme.MetricTileBackground
        };
    }

    private Panel BuildQuickActions()
    {
        var card = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(16, 14, 16, 12)
        };
        card.Paint += (_, e) => UiChrome.PaintMetricTile(card, e, UiTheme.MetricAccentStripe);

        var title = new Label
        {
            Text = "Quick actions",
            Dock = DockStyle.Top,
            Height = 28,
            Font = new Font("Segoe UI", 13F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            BackColor = UiTheme.MetricTileBackground
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(0, 8, 0, 0),
            BackColor = UiTheme.MetricTileBackground
        };
        for (var c = 0; c < 2; c++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        for (var r = 0; r < 2; r++)
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50F));

        grid.Controls.Add(QuickTile("Add customer", UiTheme.PrimaryButton, ClinicShellNavKind.Customers), 0, 0);
        grid.Controls.Add(QuickTile("Add animal", UiTheme.AccentStrip, ClinicShellNavKind.Animals), 1, 0);
        grid.Controls.Add(QuickTile("New visit", UiTheme.TealHover, ClinicShellNavKind.Visits), 0, 1);
        grid.Controls.Add(QuickTile("New treatment", UiTheme.MetricAccentSuccess, ClinicShellNavKind.Treatments), 1, 1);

        card.Controls.Add(grid);
        card.Controls.Add(title);
        return card;
    }

    private Button QuickTile(string text, Color back, ClinicShellNavKind target)
    {
        var allowed = ShellNavPermissions.CanAccess(_employee, target);
        var b = new Button
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(6),
            FlatStyle = FlatStyle.Flat,
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point),
            Cursor = allowed ? Cursors.Hand : Cursors.No,
            BackColor = allowed ? back : UiTheme.ButtonDisabledFill,
            Enabled = allowed
        };
        b.FlatAppearance.BorderSize = 0;
        b.Click += (_, _) =>
        {
            if (!allowed)
                return;
            _navigate(target);
        };
        return b;
    }

    private Panel BuildFooter()
    {
        var p = new Panel
        {
            Dock = DockStyle.Fill,
            Height = 48,
            Padding = new Padding(14, 10, 14, 10),
            BackColor = UiTheme.DemoStripBackground
        };
        p.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Welcome to ClinicVets! If you need help, contact the administrator.",
            ForeColor = UiTheme.TealSidebarDark,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = UiTheme.DemoStripBackground
        });
        return p;
    }
}
