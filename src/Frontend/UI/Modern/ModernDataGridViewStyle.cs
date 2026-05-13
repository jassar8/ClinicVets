namespace ClinicVets.Desktop.UI;

/// <summary>Applies ClinicVets v2 table chrome (no default gray WinForms grid look).</summary>
public static class ModernDataGridViewStyle
{
    public static void Apply(DataGridView dgv)
    {
        dgv.BorderStyle = BorderStyle.None;
        dgv.BackgroundColor = UiTheme.CardWhite;
        dgv.GridColor = UiTheme.CardBorder;
        dgv.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        dgv.EnableHeadersVisualStyles = false;
        dgv.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        dgv.RowHeadersVisible = false;
        dgv.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        dgv.MultiSelect = false;
        dgv.AllowUserToResizeRows = false;
        dgv.AllowUserToAddRows = false;
        dgv.AllowUserToDeleteRows = false;
        dgv.ReadOnly = true;
        dgv.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        dgv.ColumnHeadersHeight = 42;
        dgv.RowTemplate.Height = 36;
        dgv.DefaultCellStyle.Padding = new Padding(12, 8, 12, 8);
        dgv.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.GridHeaderBackground,
            ForeColor = UiTheme.GridHeaderForeColor,
            Font = new Font("Segoe UI", 10F, FontStyle.Bold, GraphicsUnit.Point),
            SelectionBackColor = UiTheme.GridHeaderBackground,
            SelectionForeColor = UiTheme.GridHeaderForeColor,
            Padding = new Padding(12, 8, 12, 8)
        };
        dgv.DefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = UiTheme.CardWhite,
            ForeColor = UiTheme.TextDark,
            SelectionBackColor = UiTheme.GridSelectionBackground,
            SelectionForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point),
            WrapMode = DataGridViewTriState.False
        };
        dgv.AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(0xFA, 0xFB, 0xFB),
            ForeColor = UiTheme.TextDark,
            SelectionBackColor = UiTheme.GridSelectionBackground,
            SelectionForeColor = UiTheme.TextDark,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point)
        };
    }
}
