using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>
/// Administrator directory view (no passwords shown).
/// </summary>
public sealed class EmployeeStaffPanel : UserControl
{
    private readonly Employee _admin;
    private readonly IEmployeeRepository _repository;
    private readonly EmployeeRegistrationService _registration;
    private readonly DataGridView _grid = new();
    private readonly ModernPrimaryButton _add = new();
    private readonly ModernOutlineButton _refresh = new();
    private readonly Label _title;
    private readonly Label _hint;

    public EmployeeStaffPanel(Employee admin, IEmployeeRepository repository, EmployeeRegistrationService registration)
    {
        _admin = admin;
        _repository = repository;
        _registration = registration;

        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        _title = UiStyles.CreateHeroTitle("Staff directory");
        _title.Margin = new Padding(0, 0, 0, 6);

        _hint = UiStyles.CreateHeroSubtitle(
            "Create clinic accounts and assign roles. Passwords are never shown here after they are saved locally.");
        _hint.Margin = new Padding(0, 0, 0, 16);

        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = UiTheme.CardWhite;
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.ColumnHeadersHeight = 40;
        _grid.RowHeadersVisible = false;
        _grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        _grid.MultiSelect = false;
        _grid.GridColor = UiTheme.CardBorder;
        _grid.Dock = DockStyle.Fill;
        _grid.EnableHeadersVisualStyles = false;
        _grid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.ContentCanvas,
            ForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point)
        };
        _grid.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.CardWhite,
            ForeColor = UiTheme.TextDark,
            SelectionBackColor = UiTheme.SidebarItemActive,
            SelectionForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            Padding = new Padding(10, 6, 10, 6)
        };

        _add.Text = "Add employee";
        _add.Margin = new Padding(0, 0, 12, 0);
        _add.Click += async (_, _) => await AddEmployeeAsync();

        _refresh.Text = "Refresh list";
        _refresh.Click += async (_, _) => await ReloadAsync();

        var buttons = new FlowLayoutPanel
        {
            FlowDirection = FlowDirection.LeftToRight,
            AutoSize = true,
            WrapContents = false,
            Margin = new Padding(0, 12, 0, 0),
            BackColor = UiTheme.CardWhite
        };
        buttons.Controls.Add(_add);
        buttons.Controls.Add(_refresh);

        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            BackColor = UiTheme.CardWhite
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        root.Controls.Add(_title, 0, 0);
        root.Controls.Add(_hint, 0, 1);
        root.Controls.Add(_grid, 0, 2);
        root.Controls.Add(buttons, 0, 3);

        Controls.Add(root);

        Load += async (_, _) => await ReloadAsync();
        Resize += (_, _) => SyncWrapWidths();
    }

    private void SyncWrapWidths()
    {
        var inner = Math.Max(320, ClientSize.Width - Padding.Horizontal);
        _title.MaximumSize = new Size(inner, 0);
        _hint.MaximumSize = new Size(inner, 0);
    }

    private async Task AddEmployeeAsync()
    {
        var owner = FindForm();
        using var dlg = new AdminCreateEmployeeForm(_admin, _registration);
        if (owner is not null && dlg.ShowDialog(owner) == DialogResult.OK)
            await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _refresh.Enabled = false;
        _add.Enabled = false;
        try
        {
            var rows = await _repository.GetAllAsync();
            _grid.Rows.Clear();
            _grid.Columns.Clear();
            _grid.Columns.Add("FullName", "Name");
            _grid.Columns.Add("Email", "Email");
            _grid.Columns.Add("Username", "Username");
            _grid.Columns.Add("Role", "Role");
            _grid.Columns.Add("Status", "Status");
            _grid.Columns.Add("EmployeeId", "Employee ID");

            foreach (var e in rows)
            {
                var username = string.IsNullOrWhiteSpace(e.Username) ? "—" : e.Username;
                var empId = string.IsNullOrWhiteSpace(e.EmployeeId) ? "—" : e.EmployeeId;
                var status = string.IsNullOrWhiteSpace(e.Status) ? "—" : e.Status;
                _grid.Rows.Add(e.FullName, e.Email, username, e.Role, status, empId);
            }
        }
        finally
        {
            _refresh.Enabled = true;
            _add.Enabled = true;
        }
    }
}
