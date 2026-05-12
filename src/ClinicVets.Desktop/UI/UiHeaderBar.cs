namespace ClinicVets.Desktop.UI;

/// <summary>Consistent top bar across login, registration, and dashboard.</summary>
public static class UiHeaderBar
{
    public static Panel Create(string subtitle)
    {
        var header = new Panel
        {
            Height = 84,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderPrimary
        };

        var stripe = new Panel
        {
            Height = 4,
            Dock = DockStyle.Bottom,
            BackColor = UiTheme.AccentStrip
        };

        var inner = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(40, 0, 40, 0)
        };

        var title = new Label
        {
            Text = "ClinicVets",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 19F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(0, 14)
        };

        var sub = new Label
        {
            Text = subtitle,
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            Location = new Point(0, 48),
            Font = new Font("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point)
        };

        inner.Controls.Add(title);
        inner.Controls.Add(sub);
        header.Controls.Add(stripe);
        header.Controls.Add(inner);

        return header;
    }
}
