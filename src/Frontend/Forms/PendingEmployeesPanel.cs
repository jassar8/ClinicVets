using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Administrator queue for self-service registrations awaiting approval; Employee IDs are assigned automatically on approval.
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

        var title = UiStyles.CreateHeroTitle("Pending Employees");
        title.Margin = new Padding(0, 0, 0, 6);

        var subtitle = UiStyles.CreateHeroSubtitle(
            "Review self-service registrations. Choose the final role, then approve or reject. The Employee ID is assigned automatically when you approve.");
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
        var wrap = new Panel
        {
            Margin = new Padding(0, 0, 0, 14),
            Height = 198,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(0)
        };
        wrap.Paint += (_, e) => UiChrome.PaintMetricTile(wrap, e, UiTheme.MetricAccentStripe);

        var bottomBar = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 76,
            BackColor = Color.Transparent
        };
        var main = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(16, 14, 16, 8),
            BackColor = Color.Transparent
        };

        wrap.Controls.Add(bottomBar);
        wrap.Controls.Add(main);

        var usernameDisplay = string.IsNullOrWhiteSpace(emp.Username) ? "—" : emp.Username.Trim();
        var statusText = string.IsNullOrWhiteSpace(emp.Status) ? EmployeeAccountStatusNames.Pending : emp.Status.Trim();

        var nameLbl = new Label
        {
            Text = emp.FullName,
            Font = new Font("Segoe UI", 12.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4),
            BackColor = Color.Transparent
        };

        var requestedRoleDisplay = string.IsNullOrWhiteSpace(emp.RequestedRole)
            ? (string.IsNullOrWhiteSpace(emp.Role) ? "—" : emp.Role.Trim())
            : emp.RequestedRole.Trim();

        var detailsLbl = new Label
        {
            Text = $"Username: {usernameDisplay}   ·   Email: {emp.Email}   ·   Requested role: {requestedRoleDisplay}",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            MaximumSize = new Size(900, 0),
            Margin = new Padding(0, 0, 0, 6),
            BackColor = Color.Transparent
        };

        var statusLbl = new Label
        {
            Text = $"Status: {statusText}",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point),
            ForeColor = UiTheme.PrimaryButton,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 0),
            BackColor = Color.Transparent
        };

        var infoGrid = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            BackColor = Color.Transparent
        };
        infoGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        infoGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        infoGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        infoGrid.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        infoGrid.Controls.Add(nameLbl, 0, 0);
        infoGrid.Controls.Add(detailsLbl, 0, 1);
        infoGrid.Controls.Add(statusLbl, 0, 2);
        main.Controls.Add(infoGrid);

        var roleCaption = new Label
        {
            Text = "Final role",
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextMuted,
            AutoSize = true,
            Margin = new Padding(0, 8, 8, 8),
            Padding = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };

        var roleCombo = new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
            Width = 160,
            Height = 32,
            Margin = new Padding(0, 4, 16, 4)
        };
        roleCombo.Items.AddRange(new object[] { EmployeeRoleNames.Secretary, EmployeeRoleNames.Veterinarian, "Administrator" });
        UiStyles.ApplyComboInner(roleCombo);
        SelectDefaultFinalRole(roleCombo, emp.Role);

        var autoIdLbl = new Label
        {
            Text = "Employee ID will be generated automatically",
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            ForeColor = UiTheme.TextDark,
            AutoSize = true,
            Margin = new Padding(0, 8, 20, 8),
            Padding = new Padding(0, 10, 0, 0),
            BackColor = Color.Transparent
        };

        var approve = new ModernPrimaryButton
        {
            Text = "Approve",
            Width = 120,
            Height = UiTheme.PrimaryButtonHeight,
            Margin = new Padding(0, 0, 10, 0)
        };

        var reject = new ModernDangerButton
        {
            Text = "Reject",
            Width = 120,
            Height = UiTheme.PrimaryButtonHeight,
            Margin = new Padding(0, 0, 0, 0)
        };

        var actions = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            AutoSize = true,
            Padding = new Padding(12, 6, 12, 6),
            BackColor = Color.Transparent
        };
        actions.Controls.Add(roleCaption);
        actions.Controls.Add(roleCombo);
        actions.Controls.Add(autoIdLbl);
        actions.Controls.Add(approve);
        actions.Controls.Add(reject);
        bottomBar.Controls.Add(actions);

        var capturedId = emp.Id;
        approve.Click += async (_, _) =>
        {
            approve.Enabled = false;
            reject.Enabled = false;
            try
            {
                var finalRole = roleCombo.SelectedItem?.ToString() ?? string.Empty;
                var (ok, message) = await _approvals.ApproveAsync(capturedId, finalRole, _admin);
                if (!ok)
                {
                    MessageBox.Show(
                        FindForm(),
                        message,
                        "ClinicVets — Pending Employees",
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
                    "ClinicVets — Pending Employees",
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
                        "ClinicVets — Pending Employees",
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

        return wrap;
    }

    private static void SelectDefaultFinalRole(ComboBox combo, string? storedRole)
    {
        if (!EmployeeRoleNames.TryParse(storedRole, out var parsed))
        {
            combo.SelectedIndex = 0;
            return;
        }

        var pick = parsed == EmployeeRole.Admin ? "Administrator" : EmployeeRoleNames.ToStoredString(parsed);
        var idx = combo.Items.IndexOf(pick);
        combo.SelectedIndex = idx >= 0 ? idx : 0;
    }
}
