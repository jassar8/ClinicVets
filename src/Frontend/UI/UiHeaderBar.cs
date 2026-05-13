using ClinicVets.Desktop;

namespace ClinicVets.Desktop.UI;

/// <summary>App header with logo, title, and subtitle.</summary>
public static class UiHeaderBar
{
    public static Panel Create(string subtitle)
    {
        var header = new Panel
        {
            Height = 88,
            Dock = DockStyle.Top,
            BackColor = UiTheme.HeaderPrimary
        };

        var stripe = new Panel
        {
            Height = 3,
            Dock = DockStyle.Bottom,
            BackColor = UiTheme.AccentStrip
        };

        var inner = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = UiTheme.HeaderPrimary,
            Padding = new Padding(32, 0, 32, 0)
        };

        var logo = new PictureBox
        {
            Size = new Size(46, 46),
            SizeMode = PictureBoxSizeMode.Zoom,
            Location = new Point(0, 18),
            BackColor = UiTheme.HeaderPrimary
        };
        try
        {
            logo.Image = AppBranding.GetHeaderImage();
        }
        catch
        {
            logo.Visible = false;
        }

        var title = new Label
        {
            Text = "ClinicVets",
            ForeColor = Color.White,
            Font = new Font("Segoe UI", 22F, FontStyle.Bold, GraphicsUnit.Point),
            AutoSize = true,
            Location = new Point(58, 16)
        };

        var sub = new Label
        {
            Text = subtitle,
            ForeColor = UiTheme.SubtitleOnHeader,
            AutoSize = true,
            Location = new Point(58, 52),
            Font = new Font("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point),
            MaximumSize = new Size(900, 0)
        };

        inner.Controls.Add(logo);
        inner.Controls.Add(title);
        inner.Controls.Add(sub);
        header.Controls.Add(stripe);
        header.Controls.Add(inner);

        return header;
    }
}
