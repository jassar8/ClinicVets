using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Administrator queue for self-service registrations awaiting approval and a four-digit employee ID.
/// </summary>
public sealed class PendingEmployeesPanel : UserControl
{
    private readonly Employee _admin;
    private readonly EmployeeApprovalService _approvals;
    private readonly FlowLayoutPanel _listHost = new()
    {
        Dock = DockStyle.Fill,
        FlowDirection = FlowDirection.TopDown,
        WrapContents = false,
        AutoScroll = true,
        BackColor = UiTheme.CardWhite,
        Padding = new Padding(0, 4, 0, 0)
    };

    public PendingEmployeesPanel(Employee admin, EmployeeApprovalService approvals)
    {
        _admin = admin;
        _approvals = approvals;

        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        var title = UiStyles.CreateHeroTitle("Pending employees");
        title.Margin = new Padding(0, 0, 0, 6);

        var subtitle = UiStyles.CreateHeroSubtitle(
            "Review self-service registrations. Approve only after assigning a unique four-digit Employee ID.");
        subtitle.Margin = new Padding(0, 0, 0, 12);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            BackColor = UiTheme.CardWhite
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        root.Controls.Add(title, 0, 0);
        root.Controls.Add(subtitle, 0, 1);
        root.Controls.Add(_listHost, 0, 2);

        Controls.Add(root);

        _listHost.SizeChanged += (_, _) => SyncRowWidths();

        Load += async (_, _) => await ReloadAsync();
        VisibleChanged += async (_, _) =>
        {
            if (Visible)
                await ReloadAsync();
        };
    }

    private void SyncRowWidths()
    {
        var w = Math.Max(360, _listHost.ClientSize.Width - _listHost.Padding.Horizontal - SystemInformation.VerticalScrollBarWidth);
        foreach (Control c in _listHost.Controls)
            c.Width = w;
    }

    private async Task ReloadAsync()
    {
        if (!IsHandleCreated)
            return;

        _listHost.SuspendLayout();
        try
        {
            _listHost.Controls.Clear();
            var pending = await _approvals.GetPendingAsync();

            if (pending.Count == 0)
            {
                var empty = new Label
                {
                    Text = "No pending employee registrations.",
                    ForeColor = UiTheme.TextMuted,
                    Font = new Font("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point),
                    AutoSize = true,
                    Margin = new Padding(4, 12, 4, 8),
                    BackColor = UiTheme.CardWhite
                };
                _listHost.Controls.Add(empty);
                return;
            }

            foreach (var emp in pending)
                _listHost.Controls.Add(BuildRow(emp));

            SyncRowWidths();
        }
        finally
        {
            _listHost.ResumeLayout(true);
        }
    }

    private Panel BuildRow(Employee emp)
    {
        var row = new Panel
        {
            Margin = new Padding(0, 0, 0, 12),
            Height = 138,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(16, 14, 16, 14)
        };
        row.Paint += (_, e) => UiChrome.PaintMetricTile(row, e, UiTheme.MetricAccentStripe);

        var nameLbl = new Label
        {
            Text = emp.FullName,
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Location = new Point(16, 14),
            BackColor = UiTheme.MetricTileBackground
        };

        var usernameDisplay = string.IsNullOrWhiteSpace(emp.Username) ? "—" : emp.Username.Trim();
        var meta = new Label
        {
            Text = $"Username: {usernameDisplay}   ·   Email: {emp.Email}   ·   Requested role: {emp.Role}",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = false,
            Location = new Point(16, 44),
            Size = new Size(Math.Max(200, row.Width - 32), 44),
            BackColor = UiTheme.MetricTileBackground
        };

        var idCaption = new Label
        {
            Text = "Employee ID",
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Location = new Point(16, 92),
            BackColor = UiTheme.MetricTileBackground
        };

        var idBox = new TextBox
        {
            MaxLength = 4,
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
            Location = new Point(110, 86),
            Width = 88,
            PlaceholderText = "0000"
        };

        var approve = new ModernPrimaryButton
        {
            Text = "Approve",
            Location = new Point(212, 80),
            Width = 118,
            Height = UiTheme.PrimaryButtonHeight
        };

        var reject = new ModernDangerButton
        {
            Text = "Reject",
            Location = new Point(338, 80),
            Width = 118,
            Height = UiTheme.PrimaryButtonHeight
        };

        var capturedId = emp.Id;
        approve.Click += async (_, _) =>
        {
            approve.Enabled = false;
            reject.Enabled = false;
            try
            {
                var (ok, message) = await _approvals.ApproveAsync(capturedId, idBox.Text, _admin);
                if (!ok)
                {
                    MessageBox.Show(
                        FindForm(),
                        message,
                        "ClinicVets — approve employee",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                await ReloadAsync();
            }
            finally
            {
                approve.Enabled = true;
                reject.Enabled = true;
            }
        };

        reject.Click += async (_, _) =>
        {
            approve.Enabled = false;
            reject.Enabled = false;
            try
            {
                var confirm = MessageBox.Show(
                    FindForm(),
                    $"Reject registration for {emp.FullName}?",
                    "ClinicVets — confirm rejection",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);
                if (confirm != DialogResult.Yes)
                    return;

                var (ok, message) = await _approvals.RejectAsync(capturedId, _admin);
                if (!ok)
                {
                    MessageBox.Show(
                        FindForm(),
                        message,
                        "ClinicVets — reject employee",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                await ReloadAsync();
            }
            finally
            {
                approve.Enabled = true;
                reject.Enabled = true;
            }
        };

        row.Controls.Add(nameLbl);
        row.Controls.Add(meta);
        row.Controls.Add(idCaption);
        row.Controls.Add(idBox);
        row.Controls.Add(approve);
        row.Controls.Add(reject);

        row.Resize += (_, _) =>
        {
            meta.Width = Math.Max(200, row.ClientSize.Width - 32);
        };

        return row;
    }
}
