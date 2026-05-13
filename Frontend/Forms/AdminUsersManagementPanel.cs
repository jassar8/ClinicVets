using System.Drawing;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public enum UsersHubTab
{
    All,
    Pending,
    Rejected
}

/// <summary>
/// Administrator users directory: metrics, filters, tabbed directory, inline approval, and embedded add-user overlay.
/// </summary>
public sealed class AdminUsersManagementPanel : UserControl
{
    private readonly Employee _admin;
    private readonly IEmployeeRepository _repository;
    private readonly EmployeeRegistrationService _registration;
    private readonly EmployeeApprovalService _approvals;

    private readonly TableLayoutPanel _root = new() { Dock = DockStyle.Fill, ColumnCount = 1, BackColor = UiTheme.CardWhite };
    private readonly Panel _statsRow = new() { Dock = DockStyle.Fill, Height = 112, BackColor = UiTheme.CardWhite };
    private readonly Panel _toolbar = new() { Dock = DockStyle.Fill, Height = 56, BackColor = UiTheme.CardWhite };
    private readonly Panel _tabBar = new() { Dock = DockStyle.Fill, Height = 44, BackColor = UiTheme.CardWhite };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill };
    private readonly Panel _actionStrip = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 108), BackColor = UiTheme.AccentMintWash };
    private readonly Panel _demoStrip = new() { Dock = DockStyle.Fill, Height = 52, BackColor = Color.FromArgb(236, 248, 252) };

    private readonly TextBox _search = new();
    private readonly ComboBox _filterStatus = new();
    private readonly ComboBox _filterRole = new();
    private readonly ModernPrimaryButton _addUser = new();

    private readonly Label _tabAll = new();
    private readonly Label _tabPending = new();
    private readonly Label _tabRejected = new();

    private readonly Label _stripTitle = new();
    private readonly TextBox _approveId = new();
    private readonly ComboBox _finalRole = new();
    private readonly ModernPrimaryButton _approve = new();
    private readonly ModernDangerButton _reject = new();
    private readonly FlowLayoutPanel _pendingActions = new();

    private readonly Panel _overlay = new() { Visible = false, BackColor = Color.FromArgb(200, 236, 244, 240) };
    private readonly AdminCreateEmployeePanel _createPanel;
    private Panel? _overlayCard;

    private UsersHubTab _hubTab = UsersHubTab.All;
    private IReadOnlyList<Employee> _all = Array.Empty<Employee>();
    private Guid? _selectedId;

    private static readonly Font TabFont = new("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);
    private static readonly Font TabFontActive = new("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);

    public AdminUsersManagementPanel(
        Employee admin,
        IEmployeeRepository repository,
        EmployeeRegistrationService registration,
        EmployeeApprovalService approvals)
    {
        _admin = admin;
        _repository = repository;
        _registration = registration;
        _approvals = approvals;

        SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint, true);
        UpdateStyles();
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        _createPanel = new AdminCreateEmployeePanel(admin, registration);
        _createPanel.Saved += async (_, _) =>
        {
            _overlay.Visible = false;
            _createPanel.ResetForm();
            await RefreshAsync();
            StaffDirectoryChanged?.Invoke(this, EventArgs.Empty);
        };
        _createPanel.Cancelled += (_, _) =>
        {
            _overlay.Visible = false;
            _createPanel.ResetForm();
        };

        BuildStatsHost();
        BuildToolbar();
        BuildTabs();
        BuildGrid();
        BuildActionStrip();
        BuildDemoStrip();

        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 58F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 46F));
        _root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 118F));
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 68F));
        _root.Controls.Add(_statsRow, 0, 0);
        _root.Controls.Add(_toolbar, 0, 1);
        _root.Controls.Add(_tabBar, 0, 2);
        _root.Controls.Add(_grid, 0, 3);
        _root.Controls.Add(_actionStrip, 0, 4);
        _root.Controls.Add(_demoStrip, 0, 5);

        Controls.Add(_root);

        _overlay.Dock = DockStyle.Fill;
        BuildOverlay();
        Controls.Add(_overlay);

        Load += async (_, _) => await RefreshAsync();
    }

    public event EventHandler? StaffDirectoryChanged;

    public UsersHubTab HubTab => _hubTab;

    public void SetHubTab(UsersHubTab tab)
    {
        _hubTab = tab;
        UpdateTabVisuals();
        _ = RefreshAsync();
    }

    public async Task RefreshAsync()
    {
        _all = await _repository.GetAllAsync();
        RebuildStatsTiles();
        ApplyFiltersToGrid();
        StaffDirectoryChanged?.Invoke(this, EventArgs.Empty);
    }

    private void BuildStatsHost()
    {
        _statsRow.Padding = new Padding(0, 0, 0, 8);
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = UiTheme.CardWhite
        };
        for (var i = 0; i < 4; i++)
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        _statsRow.Controls.Add(layout);
        _statsRow.Tag = layout;
    }

    private void RebuildStatsTiles()
    {
        if (_statsRow.Controls.Count == 0 || _statsRow.Controls[0] is not TableLayoutPanel layout)
            return;

        layout.Controls.Clear();
        var total = _all.Count;
        var pending = _all.Count(e => IsStatus(e, EmployeeAccountStatusNames.Pending));
        var approved = _all.Count(e => IsStatus(e, EmployeeAccountStatusNames.Approved));
        var rejected = _all.Count(e => IsStatus(e, EmployeeAccountStatusNames.Rejected));

        void AddTile(int col, string title, string value, Color accent, Padding margin)
        {
            var tile = CreateStatCard(title, value, accent, margin);
            tile.Dock = DockStyle.Fill;
            layout.Controls.Add(tile, col, 0);
        }

        AddTile(0, "Total users", total.ToString("D0"), UiTheme.MetricAccentStripe, new Padding(0, 0, 12, 0));
        AddTile(1, "Pending approvals", pending.ToString("D0"), Color.FromArgb(220, 140, 60), new Padding(0, 0, 12, 0));
        AddTile(2, "Approved employees", approved.ToString("D0"), Color.FromArgb(52, 148, 108), new Padding(0, 0, 12, 0));
        AddTile(3, "Rejected users", rejected.ToString("D0"), Color.FromArgb(200, 80, 80), new Padding(0, 0, 0, 0));
    }

    private static Panel CreateStatCard(string title, string value, Color accent, Padding margin)
    {
        var p = new Panel
        {
            Margin = margin,
            BackColor = UiTheme.MetricTileBackground,
            Padding = new Padding(18, 16, 16, 14)
        };
        p.Paint += (_, e) => UiChrome.PaintMetricTile(p, e, accent);

        var t = new Label
        {
            Text = title,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(18, 14),
            BackColor = UiTheme.MetricTileBackground
        };
        var v = new Label
        {
            Text = value,
            ForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 20F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(18, 38),
            BackColor = UiTheme.MetricTileBackground
        };
        p.Controls.Add(t);
        p.Controls.Add(v);
        return p;
    }

    private void BuildToolbar()
    {
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 8, 0, 4),
            BackColor = UiTheme.CardWhite
        };

        _search.PlaceholderText = "Search users…";
        _search.Width = 280;
        _search.Height = 40;
        _search.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        _search.Margin = new Padding(0, 4, 16, 4);
        _search.TextChanged += (_, _) => ApplyFiltersToGrid();

        _filterStatus.DropDownStyle = ComboBoxStyle.DropDownList;
        _filterStatus.Width = 160;
        _filterStatus.Margin = new Padding(0, 4, 12, 4);
        _filterStatus.Items.AddRange(new object[] { "All statuses", "Pending", "Approved", "Rejected" });
        _filterStatus.SelectedIndex = 0;
        UiStyles.ApplyComboInner(_filterStatus);
        _filterStatus.SelectedIndexChanged += (_, _) => ApplyFiltersToGrid();

        _filterRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _filterRole.Width = 170;
        _filterRole.Margin = new Padding(0, 4, 12, 4);
        _filterRole.Items.AddRange(new object[] { "All roles", "Secretary", "Veterinarian", "Administrator" });
        _filterRole.SelectedIndex = 0;
        UiStyles.ApplyComboInner(_filterRole);
        _filterRole.SelectedIndexChanged += (_, _) => ApplyFiltersToGrid();

        var filterCaption = new Label
        {
            Text = "Filter",
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            Margin = new Padding(0, 12, 6, 0),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _addUser.Text = "+ Add new user";
        _addUser.AutoSize = true;
        _addUser.Margin = new Padding(24, 4, 0, 4);
        _addUser.Click += (_, _) =>
        {
            _createPanel.ResetForm();
            _overlay.Visible = true;
            _overlay.BringToFront();
        };

        flow.Controls.Add(_search);
        flow.Controls.Add(filterCaption);
        flow.Controls.Add(_filterStatus);
        flow.Controls.Add(_filterRole);
        flow.Controls.Add(_addUser);
        _toolbar.Controls.Add(flow);
    }

    private void BuildTabs()
    {
        var wrap = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.CardWhite };
        var flow = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 6),
            BackColor = UiTheme.CardWhite
        };

        void StyleTab(Label l, string text, UsersHubTab tab)
        {
            l.Text = text;
            l.AutoSize = true;
            l.Margin = new Padding(0, 6, 28, 6);
            l.Cursor = Cursors.Hand;
            l.BackColor = UiTheme.CardWhite;
            l.Click += (_, _) =>
            {
                _hubTab = tab;
                UpdateTabVisuals();
                ApplyFiltersToGrid();
            };
        }

        StyleTab(_tabAll, "All users", UsersHubTab.All);
        StyleTab(_tabPending, "Pending employees", UsersHubTab.Pending);
        StyleTab(_tabRejected, "Rejected users", UsersHubTab.Rejected);

        flow.Controls.Add(_tabAll);
        flow.Controls.Add(_tabPending);
        flow.Controls.Add(_tabRejected);

        var line = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = UiTheme.CardBorder };
        wrap.Controls.Add(flow);
        wrap.Controls.Add(line);
        _tabBar.Controls.Add(wrap);
        UpdateTabVisuals();
    }

    private void UpdateTabVisuals()
    {
        void Set(Label l, UsersHubTab t)
        {
            var on = _hubTab == t;
            l.Font = on ? TabFontActive : TabFont;
            l.ForeColor = on ? UiTheme.PrimaryButton : UiTheme.TextMuted;
        }

        Set(_tabAll, UsersHubTab.All);
        Set(_tabPending, UsersHubTab.Pending);
        Set(_tabRejected, UsersHubTab.Rejected);
    }

    private void BuildGrid()
    {
        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = UiTheme.CardWhite;
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.ColumnHeadersHeight = 42;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.GridColor = UiTheme.CardBorder;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(228, 244, 238),
            ForeColor = UiTheme.HeaderPrimary,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
            SelectionBackColor = Color.FromArgb(228, 244, 238),
            SelectionForeColor = UiTheme.HeaderPrimary
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.CardWhite,
            ForeColor = UiTheme.TextDark,
            SelectionBackColor = UiTheme.AccentMintSoft,
            SelectionForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            Padding = new Padding(10, 8, 10, 8)
        };
        _grid.RowTemplate.Height = 40;
        _grid.SelectionChanged += (_, _) => SyncActionStripFromSelection();
        _grid.CellFormatting += GridOnCellFormatting;
        _grid.CellClick += GridOnCellClick;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Id", Visible = false });
        _grid.Columns.Add("FullName", "Full name");
        _grid.Columns.Add("Username", "Username");
        _grid.Columns.Add("Email", "Email");
        _grid.Columns.Add("Role", "Role");
        _grid.Columns.Add("EmployeeId", "Employee ID");
        _grid.Columns.Add("Status", "Status");
        _grid.Columns.Add("Password", "Password (Demo only)");
        _grid.Columns.Add("Actions", "Actions");
    }

    private void GridOnCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;
        var col = _grid.Columns[e.ColumnIndex].Name;
        var status = _grid.Rows[e.RowIndex].Cells["Status"].Value?.ToString() ?? string.Empty;
        var role = _grid.Rows[e.RowIndex].Cells["Role"].Value?.ToString() ?? string.Empty;

        if (col == "Role")
        {
            e.CellStyle.ForeColor = UiTheme.TextDark;
            if (role.Contains("Secretary", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.BackColor = Color.FromArgb(232, 242, 255);
            else if (role.Contains("Veterinarian", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.BackColor = Color.FromArgb(244, 236, 252);
            else if (role.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.BackColor = Color.FromArgb(255, 246, 230);
        }
        else if (col == "Status")
        {
            if (IsStatusLiteral(status, EmployeeAccountStatusNames.Approved))
            {
                e.CellStyle.ForeColor = UiTheme.SuccessText;
                e.CellStyle.SelectionForeColor = UiTheme.SuccessText;
            }
            else if (IsStatusLiteral(status, EmployeeAccountStatusNames.Pending))
            {
                e.CellStyle.ForeColor = Color.FromArgb(180, 100, 20);
                e.CellStyle.SelectionForeColor = Color.FromArgb(180, 100, 20);
            }
            else if (IsStatusLiteral(status, EmployeeAccountStatusNames.Rejected))
            {
                e.CellStyle.ForeColor = UiTheme.ErrorText;
                e.CellStyle.SelectionForeColor = UiTheme.ErrorText;
            }
        }
        else if (col == "Actions")
        {
            if (IsStatusLiteral(status, EmployeeAccountStatusNames.Pending))
            {
                e.Value = "Review";
                e.CellStyle.ForeColor = UiTheme.PrimaryButton;
                e.CellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Underline, GraphicsUnit.Point);
            }
            else
            {
                e.Value = "—";
                e.CellStyle.ForeColor = UiTheme.TextMuted;
                e.CellStyle.Font = _grid.DefaultCellStyle.Font;
            }
        }
    }

    private void GridOnCellClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Actions")
            return;
        var status = _grid.Rows[e.RowIndex].Cells["Status"].Value?.ToString() ?? string.Empty;
        if (!IsStatusLiteral(status, EmployeeAccountStatusNames.Pending))
            return;
        _grid.ClearSelection();
        _grid.Rows[e.RowIndex].Selected = true;
    }

    private void BuildActionStrip()
    {
        _actionStrip.Padding = new Padding(16, 12, 16, 12);
        _actionStrip.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, _actionStrip.Width, 0);
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = UiTheme.AccentMintWash
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38F));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62F));

        _stripTitle.Dock = DockStyle.Fill;
        _stripTitle.TextAlign = ContentAlignment.TopLeft;
        _stripTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        _stripTitle.ForeColor = UiTheme.TextDark;
        _stripTitle.Text = "Select an employee in the table. Pending accounts can be approved here with a unique four-digit Employee ID and final role.";

        _pendingActions.FlowDirection = FlowDirection.LeftToRight;
        _pendingActions.WrapContents = true;
        _pendingActions.AutoSize = true;
        _pendingActions.Dock = DockStyle.Fill;
        _pendingActions.BackColor = UiTheme.AccentMintWash;
        _pendingActions.Padding = new Padding(0, 2, 0, 0);

        var idCaption = new Label
        {
            Text = "Employee ID",
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Margin = new Padding(0, 10, 8, 0)
        };
        _approveId.MaxLength = 4;
        _approveId.Width = 88;
        _approveId.Margin = new Padding(0, 4, 14, 4);
        _approveId.PlaceholderText = "0000";

        var roleCaption = new Label
        {
            Text = "Final role",
            AutoSize = true,
            ForeColor = UiTheme.TextMuted,
            Margin = new Padding(0, 10, 8, 0)
        };
        _finalRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _finalRole.Width = 150;
        _finalRole.Margin = new Padding(0, 4, 14, 4);
        _finalRole.Items.AddRange(new object[] { EmployeeRoleNames.Secretary, EmployeeRoleNames.Veterinarian, "Administrator" });
        UiStyles.ApplyComboInner(_finalRole);

        _approve.Text = "Approve";
        _approve.Margin = new Padding(0, 4, 10, 4);
        _approve.Click += async (_, _) => await ApproveSelectedAsync();

        _reject.Text = "Reject";
        _reject.Margin = new Padding(0, 4, 0, 4);
        _reject.Click += async (_, _) => await RejectSelectedAsync();

        _pendingActions.Controls.Add(idCaption);
        _pendingActions.Controls.Add(_approveId);
        _pendingActions.Controls.Add(roleCaption);
        _pendingActions.Controls.Add(_finalRole);
        _pendingActions.Controls.Add(_approve);
        _pendingActions.Controls.Add(_reject);

        layout.Controls.Add(_stripTitle, 0, 0);
        layout.Controls.Add(_pendingActions, 1, 0);
        _actionStrip.Controls.Add(layout);

        SetPendingControlsVisible(false);
    }

    private void BuildDemoStrip()
    {
        _demoStrip.Padding = new Padding(14, 10, 14, 10);
        var info = new Label
        {
            Dock = DockStyle.Fill,
            Text =
                "Demo only - passwords are visible for testing purposes. In production, passwords are encrypted and not visible.",
            ForeColor = UiTheme.HeaderPrimary,
            Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point),
            TextAlign = ContentAlignment.MiddleLeft,
            BackColor = Color.FromArgb(236, 248, 252)
        };
        _demoStrip.Controls.Add(info);
    }

    private void BuildOverlay()
    {
        _overlayCard = new Panel
        {
            Width = 580,
            Padding = new Padding(28, 24, 28, 20),
            BackColor = UiTheme.CardWhite
        };
        _overlayCard.Paint += (_, e) =>
        {
            if (_overlayCard is null)
                return;
            UiChrome.PaintCardWithShadow(_overlayCard, e, UiTheme.CardCornerRadius);
        };

        _createPanel.Dock = DockStyle.Fill;
        _overlayCard.Controls.Add(_createPanel);

        void LayoutCard()
        {
            if (_overlayCard is null || _overlay.ClientSize.Width <= 0)
                return;
            _overlayCard.Left = Math.Max(24, (_overlay.ClientSize.Width - _overlayCard.Width) / 2);
            _overlayCard.Top = Math.Max(24, (_overlay.ClientSize.Height - _overlayCard.Height) / 2);
            _overlayCard.Height = Math.Clamp(_overlay.ClientSize.Height - 48, 520, 760);
        }

        _overlay.Resize += (_, _) => LayoutCard();
        _overlay.VisibleChanged += (_, _) =>
        {
            if (_overlay.Visible)
                LayoutCard();
        };
        _overlay.Controls.Add(_overlayCard);
        Load += (_, _) => LayoutCard();
    }

    private void ApplyFiltersToGrid()
    {
        var q = _all.AsEnumerable();
        var search = _search.Text.Trim();
        if (search.Length > 0)
        {
            q = q.Where(e =>
                e.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                e.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                (!string.IsNullOrWhiteSpace(e.Username) &&
                 e.Username.Contains(search, StringComparison.OrdinalIgnoreCase)));
        }

        var statusPick = _filterStatus.SelectedItem?.ToString() ?? "All statuses";
        if (!string.Equals(statusPick, "All statuses", StringComparison.Ordinal))
        {
            q = q.Where(e => string.Equals(NormalizeStatus(e), statusPick, StringComparison.OrdinalIgnoreCase));
        }

        var rolePick = _filterRole.SelectedItem?.ToString() ?? "All roles";
        if (!string.Equals(rolePick, "All roles", StringComparison.Ordinal))
        {
            var want = rolePick.Equals("Administrator", StringComparison.Ordinal) ? EmployeeRoleNames.Admin : rolePick;
            q = q.Where(e => RoleMatches(e, want));
        }

        switch (_hubTab)
        {
            case UsersHubTab.Pending:
                q = q.Where(e => IsStatus(e, EmployeeAccountStatusNames.Pending));
                break;
            case UsersHubTab.Rejected:
                q = q.Where(e => IsStatus(e, EmployeeAccountStatusNames.Rejected));
                break;
        }

        var list = q.OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase).ToList();
        var keepId = _selectedId;
        _grid.ClearSelection();
        _grid.Rows.Clear();
        foreach (var e in list)
        {
            var username = string.IsNullOrWhiteSpace(e.Username) ? "—" : e.Username;
            var empId = string.IsNullOrWhiteSpace(e.EmployeeId) ? "—" : e.EmployeeId;
            var status = string.IsNullOrWhiteSpace(e.Status) ? "—" : e.Status.Trim();
            var pwd = string.IsNullOrWhiteSpace(e.Password) ? "—" : e.Password;
            var idx = _grid.Rows.Add(e.Id.ToString("D"), e.FullName, username, e.Email, e.Role, empId, status, pwd, string.Empty);
            if (keepId.HasValue && e.Id == keepId.Value)
                _grid.Rows[idx].Selected = true;
        }

        SyncActionStripFromSelection();
    }

    private void SyncActionStripFromSelection()
    {
        if (_grid.SelectedRows.Count != 1)
        {
            _selectedId = null;
            _stripTitle.Text = "Select an employee in the table. Pending accounts can be approved here with a unique four-digit Employee ID and final role.";
            SetPendingControlsVisible(false);
            return;
        }

        var row = _grid.SelectedRows[0];
        if (!Guid.TryParse(row.Cells["Id"].Value?.ToString(), out var id))
        {
            _selectedId = null;
            SetPendingControlsVisible(false);
            return;
        }

        _selectedId = id;
        var emp = _all.FirstOrDefault(x => x.Id == id);
        if (emp is null)
        {
            SetPendingControlsVisible(false);
            return;
        }

        if (IsStatus(emp, EmployeeAccountStatusNames.Pending))
        {
            _stripTitle.Text =
                $"Pending: {emp.FullName}. Assign a unique four-digit Employee ID, choose the final role, then approve or reject.";
            SetPendingControlsVisible(true);
            _approveId.Text = string.Empty;
            SelectDefaultFinalRole(emp.Role);
        }
        else if (IsStatus(emp, EmployeeAccountStatusNames.Rejected))
        {
            _stripTitle.Text = $"{emp.FullName} is rejected and cannot sign in.";
            SetPendingControlsVisible(false);
        }
        else
        {
            _stripTitle.Text = $"{emp.FullName} is approved. Employee ID: {(string.IsNullOrWhiteSpace(emp.EmployeeId) ? "—" : emp.EmployeeId)}.";
            SetPendingControlsVisible(false);
        }
    }

    private void SelectDefaultFinalRole(string? storedRole)
    {
        if (!EmployeeRoleNames.TryParse(storedRole, out var parsed))
        {
            _finalRole.SelectedIndex = 0;
            return;
        }

        var pick = parsed == EmployeeRole.Admin ? "Administrator" : EmployeeRoleNames.ToStoredString(parsed);
        var idx = _finalRole.Items.IndexOf(pick);
        _finalRole.SelectedIndex = idx >= 0 ? idx : 0;
    }

    private void SetPendingControlsVisible(bool visible)
    {
        foreach (Control c in _pendingActions.Controls)
            c.Visible = visible;
    }

    private async Task ApproveSelectedAsync()
    {
        if (_selectedId is null)
            return;
        var emp = _all.FirstOrDefault(x => x.Id == _selectedId);
        if (emp is null || !IsStatus(emp, EmployeeAccountStatusNames.Pending))
            return;

        _approve.Enabled = false;
        _reject.Enabled = false;
        try
        {
            var finalRole = _finalRole.SelectedItem?.ToString() ?? string.Empty;
            var (ok, message) = await _approvals.ApproveAsync(_selectedId.Value, _approveId.Text, finalRole, _admin);
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

            await RefreshAsync();
        }
        finally
        {
            _approve.Enabled = true;
            _reject.Enabled = true;
        }
    }

    private async Task RejectSelectedAsync()
    {
        if (_selectedId is null)
            return;
        var emp = _all.FirstOrDefault(x => x.Id == _selectedId);
        if (emp is null || !IsStatus(emp, EmployeeAccountStatusNames.Pending))
            return;

        var confirm = MessageBox.Show(
            FindForm(),
            $"Reject registration for {emp.FullName}?",
            "ClinicVets — pending employee",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (confirm != DialogResult.Yes)
            return;

        _approve.Enabled = false;
        _reject.Enabled = false;
        try
        {
            var (ok, message) = await _approvals.RejectAsync(_selectedId.Value, _admin);
            if (!ok)
            {
                MessageBox.Show(FindForm(), message, "ClinicVets", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            await RefreshAsync();
        }
        finally
        {
            _approve.Enabled = true;
            _reject.Enabled = true;
        }
    }

    private static bool IsStatus(Employee e, string status) =>
        string.Equals(NormalizeStatus(e), status, StringComparison.OrdinalIgnoreCase);

    private static string NormalizeStatus(Employee e) =>
        string.IsNullOrWhiteSpace(e.Status) ? EmployeeAccountStatusNames.Pending : e.Status.Trim();

    private static bool IsStatusLiteral(string displayed, string status) =>
        string.Equals(displayed.Trim(), status, StringComparison.OrdinalIgnoreCase);

    private static bool RoleMatches(Employee e, string want)
    {
        if (want.Equals(EmployeeRoleNames.Admin, StringComparison.OrdinalIgnoreCase))
            return EmployeeRoleNames.TryParse(e.Role, out var r) && r == EmployeeRole.Admin;
        return string.Equals(e.Role?.Trim(), want, StringComparison.OrdinalIgnoreCase);
    }
}
