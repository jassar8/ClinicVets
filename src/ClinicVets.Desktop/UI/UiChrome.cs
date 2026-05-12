using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Rounded surfaces and soft shadows for a modern desktop shell.</summary>
public static class UiChrome
{
    public static void PaintCardWithShadow(Control card, PaintEventArgs e, int cornerRadius)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        const int shadow = 5;
        var body = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
        var shadowRect = new Rectangle(shadow, shadow, card.Width - 1 - shadow, card.Height - 1 - shadow);

        using (var shadowPath = RoundedRect(shadowRect, cornerRadius))
        using (var b = new SolidBrush(Color.FromArgb(18, 48, 72, 56)))
            g.FillPath(b, shadowPath);

        using (var path = RoundedRect(body, cornerRadius))
        using (var fill = new SolidBrush(UiTheme.CardWhite))
        using (var edge = new Pen(UiTheme.CardBorder, 1))
        {
            g.FillPath(fill, path);
            g.DrawPath(edge, path);
        }
    }

    /// <summary>Metric tile: rounded fill, border, and left accent stripe.</summary>
    public static void PaintMetricTile(Panel tile, PaintEventArgs e, Color accent)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, tile.Width - 1, tile.Height - 1);
        const int r = 12;
        using var path = RoundedRect(rect, r);
        using (var fill = new SolidBrush(UiTheme.MetricTileBackground))
            g.FillPath(fill, path);
        using (var border = new Pen(UiTheme.MetricTileBorder, 1))
            g.DrawPath(border, path);

        using var stripe = new Pen(accent, 4);
        g.DrawLine(stripe, 10, 14, 10, tile.Height - 14);
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
