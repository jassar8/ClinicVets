using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class CustomerAnimalsPanel : UserControl
{
    private readonly CustomerDirectoryService _customers;
    private readonly ComboBox _customerCombo = new();
    private readonly RoundedComboHost _customerHost;
    private readonly DataGridView _grid = new();
    private readonly Label _heroTitle;
    private readonly Label _heroSubtitle;

    public CustomerAnimalsPanel(CustomerDirectoryService customers)
    {
        _customers = customers;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        _heroTitle = UiStyles.CreateHeroTitle("Customer animals");
        _heroSubtitle = UiStyles.CreateHeroSubtitle(
            "Choose a registered customer to see the animals on file for that household.");

        _customerCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        UiStyles.ApplyComboInner(_customerCombo);
        _customerHost = new RoundedComboHost(_customerCombo);
        _customerCombo.SelectedIndexChanged += async (_, _) => await ReloadAnimalsAsync();

        _grid.ReadOnly = true;
        _grid.AllowUserToAddRows = false;
        _grid.AllowUserToDeleteRows = false;
        _grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        _grid.BackgroundColor = UiTheme.CardWhite;
        _grid.BorderStyle = BorderStyle.None;
        _grid.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
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
            SelectionBackColor = UiTheme.SidebarNavActive,
            SelectionForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            Padding = new Padding(10, 6, 10, 6)
        };

        var top = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(0, 0, 0, 12),
            BackColor = UiTheme.CardWhite
        };
        top.Controls.Add(_heroTitle);
        top.Controls.Add(_heroSubtitle);
        top.Controls.Add(UiStyles.CreateFieldCaption("Customer"));
        top.Controls.Add(_customerHost);

        var root = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.CardWhite };
        root.Controls.Add(top);
        root.Controls.Add(_grid);

        Controls.Add(root);

        Load += async (_, _) => await LoadCustomersAsync();
    }

    private async Task LoadCustomersAsync()
    {
        var list = await _customers.ListCustomersAsync();
        _customerCombo.Items.Clear();
        foreach (var c in list)
            _customerCombo.Items.Add(c);

        if (_customerCombo.Items.Count > 0)
            _customerCombo.SelectedIndex = 0;
        else
            await ReloadAnimalsAsync();
    }

    private async Task ReloadAnimalsAsync()
    {
        if (_customerCombo.SelectedItem is not Customer c)
        {
            _grid.Rows.Clear();
            _grid.Columns.Clear();
            return;
        }

        var animals = await _customers.GetAnimalsForCustomerAsync(c.Id);
        _grid.Rows.Clear();
        _grid.Columns.Clear();
        _grid.Columns.Add("Name", "Animal name");
        _grid.Columns.Add("Species", "Species");

        foreach (var a in animals)
            _grid.Rows.Add(a.Name, a.Species);
    }
}
