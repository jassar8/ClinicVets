using System.Drawing;
using System.Drawing.Drawing2D;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
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
        Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.CardWhite,
            Padding = new Padding(0, 0, 0, 0)
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

    private Control BuildMetricsRow()
    {
        var stack = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.CardWhite,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        stack.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = UiTheme.CardWhite,
            MinimumSize = new Size(0, 118)
        };
        for (var i = 0; i < 4; i++)
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        grid.Controls.Add(HomeMetricCard("Today's appointments", "8", UiTheme.PrimaryButton, null), 0, 0);
        grid.Controls.Add(HomeMetricCard("Active animals", "—", UiTheme.MetricAccentPending, _metricAnimals), 1, 0);
        grid.Controls.Add(HomeMetricCard("Registered customers", "—", UiTheme.AccentStrip, _metricCustomers), 2, 0);
        grid.Controls.Add(HomeMetricCard("Treatments given", "23", UiTheme.MetricAccentSuccess, null), 3, 0);

        var foot = new Label
        {
            Text = "Sample KPIs shown where noted · Customer and animal totals update from your directory.",
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 8, 0, 4),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = UiTheme.CardWhite,
            UseCompatibleTextRendering = false
        };

        stack.Controls.Add(grid, 0, 0);
        stack.Controls.Add(foot, 0, 1);
        return stack;
    }

    private static Panel HomeMetricCard(string title, string initialValue, Color accent, Label? bindValue)
    {
        var outer = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, UiTheme.Layout.SectionGap, 0),
            Padding = new Padding(12, 10, 12, 10),
            BackColor = UiTheme.MetricTileBackground,
            MinimumSize = new Size(80, 96)
        };
        outer.Paint += (_, e) => UiChrome.PaintMetricTile(outer, e, accent);

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            BackColor = UiTheme.MetricTileBackground
        };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 40F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        grid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var icon = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 8, 0),
            BackColor = UiTheme.MetricTileBackground
        };
        icon.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var b = new SolidBrush(Color.FromArgb(210, accent));
            var r = icon.ClientRectangle;
            var inset = 2;
            e.Graphics.FillEllipse(b, r.Left + inset, r.Top + inset, Math.Max(8, r.Width - inset * 2), Math.Max(8, r.Height - inset * 2));
        };

        var t = new Label
        {
            Text = title,
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            BackColor = UiTheme.MetricTileBackground,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            UseCompatibleTextRendering = false,
            MaximumSize = new Size(240, 0)
        };

        var value = bindValue ?? new Label();
        value.Text = initialValue;
        value.AutoSize = true;
        value.TextAlign = ContentAlignment.MiddleLeft;
        value.Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point);
        value.ForeColor = UiTheme.TextDark;
        value.BackColor = UiTheme.MetricTileBackground;
        value.Dock = DockStyle.Fill;
        value.Margin = new Padding(0, 4, 0, 0);
        value.UseCompatibleTextRendering = false;

        grid.Controls.Add(icon, 0, 0);
        grid.SetRowSpan(icon, 2);
        grid.Controls.Add(t, 1, 0);
        grid.Controls.Add(value, 1, 1);

        outer.Controls.Add(grid);
        return outer;
    }

    private Control BuildBody()
    {
        var split = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.CardWhite
        };
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60F));
        split.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));

        split.Controls.Add(BuildScheduleCard(), 0, 0);
        split.Controls.Add(BuildRightColumn(), 1, 0);
        return split;
    }

    private Control BuildScheduleCard()
    {
        var outer = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, UiTheme.Layout.SectionGap, 0),
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(16, 14, 16, 12)
        };
        outer.Paint += (_, e) => UiChrome.PaintMetricTile(outer, e, UiTheme.MetricAccentStripe);

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.MetricTileBackground
        };
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var head = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            BackColor = UiTheme.MetricTileBackground,
            Margin = new Padding(0, 0, 0, 8)
        };
        head.Controls.Add(new Label
        {
            Text = "Today's schedule",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Margin = new Padding(0, 4, 12, 4),
            BackColor = UiTheme.MetricTileBackground,
            UseCompatibleTextRendering = false
        });
        var cal = new ModernOutlineButton { Text = "View calendar", Margin = new Padding(0, 2, 0, 2) };
        cal.Click += (_, _) => _navigate(ClinicShellNavKind.Visits);
        head.Controls.Add(cal);

        var rows = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 0, 0, 4),
            BackColor = UiTheme.MetricTileBackground
        };
        rows.Controls.Add(MakeScheduleRow("10:00 AM", "Buddy", "Golden Retriever", "Vaccination", "Scheduled"));
        rows.Controls.Add(MakeScheduleRow("11:30 AM", "Luna", "Siamese cat", "Check-up", "Completed"));
        rows.Controls.Add(MakeScheduleRow("02:15 PM", "Rocky", "German Shepherd", "Follow-up", "Scheduled"));

        void SyncRowWidths()
        {
            var w = Math.Max(200, rows.ClientSize.Width - rows.Padding.Horizontal);
            foreach (Control c in rows.Controls)
                c.Width = w;
        }

        rows.SizeChanged += (_, _) => SyncRowWidths();
        rows.HandleCreated += (_, _) => SyncRowWidths();

        var newVisit = new ModernPrimaryButton
        {
            Text = "+  New visit",
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 12, 0, 0)
        };
        newVisit.Click += (_, _) => _navigate(ClinicShellNavKind.Visits);

        inner.Controls.Add(head, 0, 0);
        inner.Controls.Add(rows, 0, 1);
        inner.Controls.Add(newVisit, 0, 2);
        outer.Controls.Add(inner);
        return outer;
    }

    private static Control MakeScheduleRow(string time, string pet, string breed, string service, string status)
    {
        const int timeColW = 108;
        var row = new TableLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            MinimumSize = new Size(0, 58),
            Margin = new Padding(0, 0, 0, 10),
            ColumnCount = 3,
            RowCount = 1,
            BackColor = UiTheme.CardWhite,
            Padding = new Padding(12, 10, 12, 10)
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, timeColW));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        row.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, row.Width - 1, row.Height - 1);
        };

        var timeBox = new Label
        {
            Text = time,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = Color.White,
            TextAlign = ContentAlignment.MiddleCenter,
            BackColor = UiTheme.PrimaryButton,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 2, 10, 2),
            UseCompatibleTextRendering = false,
            AutoSize = false
        };

        var pending = status.Equals("Scheduled", StringComparison.OrdinalIgnoreCase);
        var pill = new Label
        {
            Text = status,
            AutoSize = true,
            Padding = new Padding(10, 6, 10, 6),
            Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = pending ? UiTheme.TealMain : UiTheme.SuccessText,
            BackColor = pending ? UiTheme.AccentMintWash : UiTheme.SuccessBackground,
            Margin = new Padding(10, 0, 0, 0),
            TextAlign = ContentAlignment.MiddleCenter,
            UseCompatibleTextRendering = false
        };

        var mid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.CardWhite
        };
        mid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        mid.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var petLine = new Label
        {
            Text = $"{pet}   ·   {breed}",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.BottomLeft,
            BackColor = UiTheme.CardWhite,
            UseCompatibleTextRendering = false,
            MaximumSize = new Size(900, 0)
        };
        var svcLine = new Label
        {
            Text = service,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopLeft,
            BackColor = UiTheme.CardWhite,
            UseCompatibleTextRendering = false,
            MaximumSize = new Size(900, 0)
        };
        mid.Controls.Add(petLine, 0, 0);
        mid.Controls.Add(svcLine, 0, 1);

        void SyncDetailWidths()
        {
            var pillW = pill.PreferredSize.Width + pill.Margin.Horizontal + 4;
            var mw = Math.Max(120, row.ClientSize.Width - row.Padding.Horizontal - timeColW - pillW);
            petLine.MaximumSize = new Size(mw, 0);
            svcLine.MaximumSize = new Size(mw, 0);
        }

        row.Layout += (_, _) => SyncDetailWidths();
        row.SizeChanged += (_, _) => SyncDetailWidths();
        row.HandleCreated += (_, _) => SyncDetailWidths();

        row.Controls.Add(timeBox, 0, 0);
        row.Controls.Add(mid, 1, 0);
        row.Controls.Add(pill, 2, 0);
        return row;
    }

    private Control BuildRightColumn()
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

    private Control BuildActivitiesCard()
    {
        var outer = new Panel
        {
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 0, 0, UiTheme.Layout.SectionGap),
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(14, 12, 14, 12)
        };
        outer.Paint += (_, e) => UiChrome.PaintMetricTile(outer, e, UiTheme.AccentStrip);

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.MetricTileBackground
        };
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        var title = new Label
        {
            Text = "Recent activities",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = UiTheme.MetricTileBackground,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 6),
            UseCompatibleTextRendering = false
        };

        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            Padding = new Padding(0, 2, 0, 4),
            BackColor = UiTheme.MetricTileBackground
        };
        flow.Controls.Add(MakeActivity("●  New customer registered: John Smith", "10 minutes ago"));
        flow.Controls.Add(MakeActivity("●  Visit completed for Buddy", "32 minutes ago"));
        flow.Controls.Add(MakeActivity("●  Treatment logged for Luna", "1 hour ago"));

        void SyncAct()
        {
            var mw = Math.Max(120, flow.ClientSize.Width - flow.Padding.Horizontal);
            foreach (Control c in flow.Controls)
            {
                if (c is Label l)
                    l.MaximumSize = new Size(mw, 0);
            }
        }

        flow.SizeChanged += (_, _) => SyncAct();
        flow.HandleCreated += (_, _) => SyncAct();

        var all = new ModernOutlineButton { Text = "View all", Dock = DockStyle.Fill, Margin = new Padding(0, 8, 0, 0) };
        all.Click += (_, _) => _navigate(ClinicShellNavKind.Customers);

        inner.Controls.Add(title, 0, 0);
        inner.Controls.Add(flow, 0, 1);
        inner.Controls.Add(all, 0, 2);
        outer.Controls.Add(inner);
        return outer;
    }

    private static Label MakeActivity(string text, string when) =>
        new()
        {
            Text = text + Environment.NewLine + when,
            AutoSize = true,
            Margin = new Padding(0, 4, 0, 0),
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            BackColor = UiTheme.MetricTileBackground,
            UseCompatibleTextRendering = false
        };

    private Control BuildQuickActions()
    {
        var outer = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(14, 12, 14, 12)
        };
        outer.Paint += (_, e) => UiChrome.PaintMetricTile(outer, e, UiTheme.MetricAccentStripe);

        var inner = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.MetricTileBackground
        };
        inner.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        inner.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var title = new Label
        {
            Text = "Quick actions",
            Dock = DockStyle.Fill,
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            BackColor = UiTheme.MetricTileBackground,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 8),
            UseCompatibleTextRendering = false
        };

        var grid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
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

        inner.Controls.Add(title, 0, 0);
        inner.Controls.Add(grid, 0, 1);
        outer.Controls.Add(inner);
        return outer;
    }

    private ModernPrimaryButton QuickTile(string text, Color back, ClinicShellNavKind target)
    {
        var allowed = ShellNavPermissions.CanAccess(_employee, target);
        var b = new ModernPrimaryButton
        {
            Text = text,
            Dock = DockStyle.Fill,
            Margin = new Padding(8, 6, 8, 6),
            Cursor = allowed ? Cursors.Hand : Cursors.No,
            Enabled = allowed,
            AccentOverride = allowed ? back : null
        };
        b.Click += (_, _) =>
        {
            if (!allowed)
                return;
            _navigate(target);
        };
        return b;
    }

    private Control BuildFooter()
    {
        var p = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 1,
            Padding = new Padding(12, 8, 12, 8),
            BackColor = UiTheme.DemoStripBackground,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink
        };
        p.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        p.Controls.Add(new Label
        {
            Dock = DockStyle.Fill,
            Text = "Welcome to ClinicVets! If you need help, contact the administrator.",
            ForeColor = UiTheme.TealSidebarDark,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = UiTheme.DemoStripBackground,
            AutoSize = true,
            UseCompatibleTextRendering = false
        }, 0, 0);
        return p;
    }
}
