using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>Static sample visits for quick-access demo (no persistence layer yet).</summary>
public sealed class DemoVisitsPanel : UserControl
{
    public DemoVisitsPanel()
    {
        Dock = DockStyle.Fill;
        BackColor = UiTheme.CardWhite;
        Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

        var title = UiStyles.CreateHeroTitle("Visits (sample data)");
        title.Margin = new Padding(0, 0, 0, 6);

        var subtitle = UiStyles.CreateHeroSubtitle(
            "These rows are for presentation only. A full visits module will connect to stored appointments later.");
        subtitle.Margin = new Padding(0, 0, 0, 16);

        var grid = new DataGridView { Dock = DockStyle.Fill };
        ModernDataGridViewStyle.Apply(grid);
        grid.Columns.Add("When", "Date & time");
        grid.Columns.Add("Pet", "Pet");
        grid.Columns.Add("Customer", "Customer");
        grid.Columns.Add("Reason", "Reason");
        grid.Columns.Add("Status", "Status");

        grid.Rows.Add("Today, 10:00 AM", "Buddy", "Sarah Johnson", "Vaccination", "Scheduled");
        grid.Rows.Add("Today, 11:30 AM", "Luna", "Sarah Johnson", "Check-up", "Completed");
        grid.Rows.Add("Today, 2:15 PM", "Felix", "Maria Garcia", "Follow-up", "Scheduled");
        grid.Rows.Add("Tomorrow, 9:00 AM", "Coco", "Ahmed Hassan", "Wing trim", "Scheduled");
        grid.Rows.Add("May 16, 3:30 PM", "Buddy", "Sarah Johnson", "Dental", "Scheduled");

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
        root.Controls.Add(grid, 0, 2);

        Controls.Add(root);
    }
}
