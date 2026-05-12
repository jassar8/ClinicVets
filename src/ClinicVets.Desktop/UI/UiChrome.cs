using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>
/// Drawing helpers for a softer, app-style shell (inspired by the page-log-in centered card look).
/// </summary>
public static class UiChrome
{
    public static void PaintCardWithShadow(Control card, PaintEventArgs e, int cornerRadius)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        const int shadow = 4;
        var body = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
        var shadowRect = new Rectangle(shadow, shadow, card.Width - 1 - shadow, card.Height - 1 - shadow);

        using (var shadowPath = RoundedRect(shadowRect, cornerRadius))
        using (var b = new SolidBrush(Color.FromArgb(22, 60, 100, 14)))
            g.FillPath(b, shadowPath);

        using (var path = RoundedRect(body, cornerRadius))
        using (var fill = new SolidBrush(UiTheme.CardWhite))
        using (var edge = new Pen(UiTheme.CardBorder, 1))
        {
            g.FillPath(fill, path);
            g.DrawPath(edge, path);
        }
    }

    private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
    {
        var d = radius * 2;
        var path = new GraphicsPath();
        if (bounds.Width <= 0 || bounds.Height <= 0)
            return path;

        radius = Math.Min(radius, Math.Min(bounds.Width, bounds.Height) / 2);
        path.AddArc(bounds.Left, bounds.Top, d, d, 180, 90);
        path.AddArc(bounds.Right - d, bounds.Top, d, d, 270, 90);
        path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
        path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }
}
