using System.Drawing;
using System.Drawing.Drawing2D;
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
    private readonly Panel _actionStrip = new() { Dock = DockStyle.Fill, MinimumSize = new Size(0, 132), BackColor = UiTheme.AccentMintWash };
    private readonly Panel _demoStrip = new() { Dock = DockStyle.Fill, Height = 52, BackColor = UiTheme.DemoStripBackground };

    private readonly TextBox _search = new();
    private readonly ComboBox _filterStatus = new();
    private readonly ComboBox _filterRole = new();
    private readonly ModernPrimaryButton _addUser = new();

    private readonly Label _tabAll = new();
    private readonly Label _tabPending = new();
    private readonly Label _tabRejected = new();

    private readonly Label _stripTitle = new();
    private readonly Label _autoIdNotice = new();
    private readonly ComboBox _finalRole = new();
    private readonly ModernPrimaryButton _approve = new();
    private readonly ModernDangerButton _reject = new();
    private readonly TableLayoutPanel _approvalRow = new();

    private readonly Panel _overlay = new() { Visible = false, BackColor = UiTheme.OverlayScrim };
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
        _root.RowStyles.Add(new RowStyle(SizeType.Absolute, 148F));
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
        AddTile(1, "Pending approvals", pending.ToString("D0"), UiTheme.MetricAccentPending, new Padding(0, 0, 12, 0));
        AddTile(2, "Approved employees", approved.ToString("D0"), UiTheme.MetricAccentSuccess, new Padding(0, 0, 12, 0));
        AddTile(3, "Rejected users", rejected.ToString("D0"), UiTheme.MetricAccentDanger, new Padding(0, 0, 0, 0));
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
            BackColor = UiTheme.GridHeaderBackground,
            ForeColor = UiTheme.GridHeaderForeColor,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
            SelectionBackColor = UiTheme.GridHeaderBackground,
            SelectionForeColor = UiTheme.GridHeaderForeColor
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
        _grid.RowTemplate.Height = 44;
        _grid.SelectionChanged += (_, _) => SyncActionStripFromSelection();
        _grid.CellFormatting += GridOnCellFormatting;
        _grid.CellContentClick += GridOnCellContentClick;
        _grid.CellPainting += GridOnCellPainting;

        _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Id", HeaderText = "Id", Visible = false });
        _grid.Columns.Add("FullName", "Full name");
        _grid.Columns.Add("Username", "Username");
        _grid.Columns.Add("Email", "Email");
        _grid.Columns.Add("Role", "Role");
        _grid.Columns.Add("EmployeeId", "Employee ID");
        _grid.Columns.Add("Status", "Status");
        _grid.Columns.Add("Password", "Password (Demo only)");
        _grid.Columns.Add(new DataGridViewButtonColumn
        {
            Name = "Actions",
            HeaderText = "Actions",
            FlatStyle = FlatStyle.Flat,
            UseColumnTextForButtonValue = false,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
            Width = 108,
            MinimumWidth = 96,
            DefaultCellStyle = new DataGridViewCellStyle
            {
                Alignment = DataGridViewContentAlignment.MiddleCenter,
                Padding = new Padding(8, 6, 8, 6)
            }
        });
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
                e.CellStyle.BackColor = UiTheme.GridRoleSecretaryTint;
            else if (role.Contains("Veterinarian", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.BackColor = UiTheme.GridRoleVetTint;
            else if (role.Contains("Admin", StringComparison.OrdinalIgnoreCase))
                e.CellStyle.BackColor = UiTheme.GridRoleAdminTint;
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
                e.CellStyle.ForeColor = UiTheme.WarningText;
                e.CellStyle.SelectionForeColor = UiTheme.WarningText;
            }
            else if (IsStatusLiteral(status, EmployeeAccountStatusNames.Rejected))
            {
                e.CellStyle.ForeColor = UiTheme.ErrorText;
                e.CellStyle.SelectionForeColor = UiTheme.ErrorText;
            }
        }
        else if (col == "Actions")
        {
            var softDeleteFill = UiTheme.ActionSoftDeleteFill;
            var reviewFill = UiTheme.ActionReviewFill;
            var neutralFill = UiTheme.GridNeutralFill;

            if (IsStatusLiteral(status, EmployeeAccountStatusNames.Pending))
            {
                e.Value = "Review";
                e.CellStyle.BackColor = reviewFill;
                e.CellStyle.ForeColor = UiTheme.PrimaryButton;
                e.CellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
                e.CellStyle.SelectionBackColor = reviewFill;
                e.CellStyle.SelectionForeColor = UiTheme.PrimaryButton;
            }
            else if (IsStatusLiteral(status, EmployeeAccountStatusNames.Rejected))
            {
                e.Value = "Delete";
                e.CellStyle.BackColor = softDeleteFill;
                e.CellStyle.ForeColor = Color.White;
                e.CellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold, GraphicsUnit.Point);
                e.CellStyle.SelectionBackColor = softDeleteFill;
                e.CellStyle.SelectionForeColor = Color.White;
            }
            else
            {
                e.Value = "—";
                e.CellStyle.BackColor = neutralFill;
                e.CellStyle.ForeColor = UiTheme.TextMuted;
                e.CellStyle.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
                e.CellStyle.SelectionBackColor = neutralFill;
                e.CellStyle.SelectionForeColor = UiTheme.TextMuted;
            }

            if (_grid.Rows[e.RowIndex].Cells[e.ColumnIndex] is DataGridViewButtonCell btn)
                btn.FlatStyle = FlatStyle.Flat;
        }
    }

    private void GridOnCellPainting(object? sender, DataGridViewCellPaintingEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Actions")
            return;
        var status = _grid.Rows[e.RowIndex].Cells["Status"].Value?.ToString() ?? string.Empty;
        if (!IsStatusLiteral(status, EmployeeAccountStatusNames.Rejected))
            return;

        e.Handled = true;
        e.Paint(e.CellBounds, DataGridViewPaintParts.Background | DataGridViewPaintParts.SelectionBackground);

        var g = e.Graphics;
        if (g is null)
            return;

        g.SmoothingMode = SmoothingMode.AntiAlias;
        var inset = Rectangle.Inflate(e.CellBounds, -7, -8);
        if (inset.Width < 2 || inset.Height < 2)
            return;

        var radius = Math.Min(10, Math.Min(inset.Width, inset.Height) / 2);
        using var path = UiChrome.CreateRoundRectPath(inset, radius);
        var fill = UiTheme.ActionSoftDeleteFill;
        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);

        var drawFont = e.CellStyle?.Font ?? _grid.Font ?? SystemFonts.DefaultFont;
        TextRenderer.DrawText(
            g,
            "Delete",
            drawFont,
            inset,
            Color.White,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.SingleLine);

        using var line = new Pen(_grid.GridColor, 1);
        var y = e.CellBounds.Bottom - 1;
        g.DrawLine(line, e.CellBounds.Left, y, e.CellBounds.Right, y);
    }

    private async void GridOnCellContentClick(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex < 0 || _grid.Columns[e.ColumnIndex].Name != "Actions")
            return;

        var status = _grid.Rows[e.RowIndex].Cells["Status"].Value?.ToString() ?? string.Empty;
        if (IsStatusLiteral(status, EmployeeAccountStatusNames.Pending))
        {
            _grid.ClearSelection();
            _grid.Rows[e.RowIndex].Selected = true;
            return;
        }

        if (!IsStatusLiteral(status, EmployeeAccountStatusNames.Rejected))
            return;

        if (!Guid.TryParse(_grid.Rows[e.RowIndex].Cells["Id"].Value?.ToString(), out var id))
            return;

        try
        {
            await DeleteRejectedRecordAsync(id);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                FindForm(),
                ex.Message,
                "ClinicVets",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private async Task DeleteRejectedRecordAsync(Guid id)
    {
        if (!RolePermissions.IsAdministrator(_admin))
        {
            MessageBox.Show(
                FindForm(),
                "Only an administrator can delete rejected employee records.",
                "ClinicVets",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var emp = _all.FirstOrDefault(x => x.Id == id);
        if (emp is null || !IsStatus(emp, EmployeeAccountStatusNames.Rejected))
            return;

        var confirm = MessageBox.Show(
            FindForm(),
            "Are you sure you want to permanently delete this rejected employee record?",
            "ClinicVets — delete rejected record",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);
        if (confirm != DialogResult.Yes)
            return;

        var removed = await _repository.DeleteRejectedEmployeeAsync(id);
        if (!removed)
        {
            MessageBox.Show(
                FindForm(),
                "This record could not be deleted. It may no longer be rejected or may have already been removed.",
                "ClinicVets",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        await RefreshAsync();
    }

    private void BuildActionStrip()
    {
        _actionStrip.Padding = new Padding(16, 14, 16, 16);
        _actionStrip.Paint += (_, e) =>
        {
            using var pen = new Pen(UiTheme.CardBorder, 1);
            e.Graphics.DrawLine(pen, 0, 0, _actionStrip.Width, 0);
        };

        var outer = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            BackColor = UiTheme.AccentMintWash
        };
        outer.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 72F));

        _stripTitle.Dock = DockStyle.Fill;
        _stripTitle.TextAlign = ContentAlignment.TopLeft;
        _stripTitle.Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);
        _stripTitle.ForeColor = UiTheme.TextDark;
        _stripTitle.Margin = new Padding(0, 0, 0, 10);
        _stripTitle.AutoSize = true;
        _stripTitle.MaximumSize = new Size(2000, 0);
        _stripTitle.Text =
            "Select an employee in the table. Pending accounts can be approved here; the Employee ID is assigned automatically when you approve.";

        _approvalRow.Dock = DockStyle.Fill;
        _approvalRow.ColumnCount = 6;
        _approvalRow.RowCount = 1;
        _approvalRow.BackColor = UiTheme.AccentMintWash;
        _approvalRow.Padding = new Padding(0, 4, 0, 4);
        _approvalRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 340F));
        _approvalRow.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _approvalRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 216F));
        _approvalRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 168F));
        _approvalRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 16F));
        _approvalRow.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 168F));
        _approvalRow.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        _autoIdNotice.Text = "Employee ID will be generated automatically";
        _autoIdNotice.AutoSize = false;
        _autoIdNotice.Width = 340;
        _autoIdNotice.Height = 46;
        _autoIdNotice.Margin = new Padding(0, 0, 20, 0);
        _autoIdNotice.Anchor = AnchorStyles.Left;
        _autoIdNotice.TextAlign = ContentAlignment.MiddleLeft;
        _autoIdNotice.ForeColor = UiTheme.TextDark;
        _autoIdNotice.Font = new Font("Segoe UI", 11.5F, FontStyle.Regular, GraphicsUnit.Point);
        _autoIdNotice.BackColor = UiTheme.AccentMintWash;

        var roleCaption = new Label
        {
            Text = "Final role",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            ForeColor = UiTheme.TextMuted,
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
            TextAlign = ContentAlignment.MiddleLeft,
            Margin = new Padding(0, 0, 10, 0),
            BackColor = UiTheme.AccentMintWash
        };

        _finalRole.DropDownStyle = ComboBoxStyle.DropDownList;
        _finalRole.Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
        _finalRole.ItemHeight = 38;
        _finalRole.Height = 46;
        _finalRole.Width = 200;
        _finalRole.Margin = new Padding(0, 0, 24, 0);
        _finalRole.Anchor = AnchorStyles.Left;
        _finalRole.Items.AddRange(new object[] { EmployeeRoleNames.Secretary, EmployeeRoleNames.Veterinarian, "Administrator" });
        UiStyles.ApplyComboInner(_finalRole);

        const int approvalBtnH = 52;
        const int approvalBtnW = 156;
        var actionFont = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);

        _approve.AccentOverride = UiTheme.SuccessGreen;
        _approve.Text = "Approve";
        _approve.Font = actionFont;
        _approve.AutoSize = false;
        _approve.Width = approvalBtnW;
        _approve.Height = approvalBtnH;
        _approve.Margin = new Padding(0, 0, 0, 0);
        _approve.Anchor = AnchorStyles.Left;
        _approve.Click += async (_, _) => await ApproveSelectedAsync();

        _reject.Text = "Reject";
        _reject.Font = actionFont;
        _reject.AutoSize = false;
        _reject.Width = approvalBtnW;
        _reject.Height = approvalBtnH;
        _reject.Margin = new Padding(0, 0, 0, 0);
        _reject.Anchor = AnchorStyles.Left;
        _reject.Click += async (_, _) => await RejectSelectedAsync();

        _approvalRow.Controls.Add(_autoIdNotice, 0, 0);
        _approvalRow.Controls.Add(roleCaption, 1, 0);
        _approvalRow.Controls.Add(_finalRole, 2, 0);
        _approvalRow.Controls.Add(_approve, 3, 0);
        _approvalRow.Controls.Add(new Panel { Width = 16, BackColor = UiTheme.AccentMintWash, Dock = DockStyle.Fill }, 4, 0);
        _approvalRow.Controls.Add(_reject, 5, 0);

        outer.Controls.Add(_stripTitle, 0, 0);
        outer.Controls.Add(_approvalRow, 0, 1);
        _actionStrip.Controls.Add(outer);

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
            BackColor = UiTheme.DemoStripBackground
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
            _stripTitle.Text = "Select an employee in the table. Pending accounts can be approved here; the Employee ID is assigned automatically when you approve.";
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
                $"Pending: {emp.FullName}. Choose the final role, then approve or reject. The Employee ID will be generated automatically.";
            SetPendingControlsVisible(true);
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
        foreach (Control c in _approvalRow.Controls)
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
            var (ok, message) = await _approvals.ApproveAsync(_selectedId.Value, finalRole, _admin);
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
