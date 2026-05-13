using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

public sealed class CustomerSearchPanel : UserControl
{
    private readonly CustomerDirectoryService _customers;
    private readonly TextBox _query = new();
    private readonly RoundedInputHost _queryHost;
    private readonly ModernPrimaryButton _search = new();
    private readonly FeedbackBannerPanel _feedback = new();
    private readonly DataGridView _grid = new();
    private readonly Label _heroTitle;
    private readonly Label _heroSubtitle;

    public CustomerSearchPanel(CustomerDirectoryService customers)
    {
        _customers = customers;
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        _heroTitle = UiStyles.CreateHeroTitle("Customer search");
        _heroSubtitle = UiStyles.CreateHeroSubtitle(
            "Search by name, national ID, email, or phone. Leave the box empty and search to list every customer.");

        _query.PlaceholderText = "Type to filter…";
        _queryHost = new RoundedInputHost(_query);

        _search.Text = "Search";
        _search.Margin = new Padding(0, 12, 0, 0);
        _search.Click += async (_, _) => await RunSearchAsync();

        _feedback.Clear();

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
            Padding = new Padding(0, 0, 0, 8),
            BackColor = UiTheme.CardWhite
        };
        top.Controls.Add(_heroTitle);
        top.Controls.Add(_heroSubtitle);
        top.Controls.Add(UiStyles.CreateFieldCaption("Search"));
        top.Controls.Add(_queryHost);
        top.Controls.Add(_search);
        top.Controls.Add(_feedback);

        _grid.Dock = DockStyle.Fill;

        var root = new Panel { Dock = DockStyle.Fill, BackColor = UiTheme.CardWhite };
        root.Controls.Add(top);
        root.Controls.Add(_grid);

        Controls.Add(root);

        Load += async (_, _) => await RunSearchAsync();
    }

    private async Task RunSearchAsync()
    {
        _search.Enabled = false;
        try
        {
            _feedback.Clear();
            var rows = await _customers.SearchCustomersAsync(_query.Text);
            _grid.Rows.Clear();
            _grid.Columns.Clear();
            _grid.Columns.Add("FullName", "Full name");
            _grid.Columns.Add("NationalId", "National ID");
            _grid.Columns.Add("Email", "Email");
            _grid.Columns.Add("Phone", "Phone");

            foreach (var c in rows)
                _grid.Rows.Add(c.FullName, c.NationalId, c.Email, c.Phone);

            if (rows.Count == 0)
                _feedback.ShowMessage(UiFeedbackKind.None, "No customers matched your search.");
        }
        finally
        {
            _search.Enabled = true;
        }
    }
}
