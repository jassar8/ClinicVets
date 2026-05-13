using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Rounded surfaces, shadows, and paths for a modern WinForms shell.</summary>
public static class UiChrome
{
    public static GraphicsPath CreateRoundRectPath(Rectangle bounds, int radius)
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

    public static void PaintCardWithShadow(Control card, PaintEventArgs e, int cornerRadius)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        const int shadow = 6;
        var body = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
        var shadowRect = new Rectangle(shadow, shadow, card.Width - 1 - shadow, card.Height - 1 - shadow);

        using (var shadowPath = CreateRoundRectPath(shadowRect, cornerRadius))
        using (var b = new SolidBrush(Color.FromArgb(14, 40, 72, 52)))
            g.FillPath(b, shadowPath);

        using (var path = CreateRoundRectPath(body, cornerRadius))
        using (var fill = new SolidBrush(UiTheme.CardWhite))
        using (var edge = new Pen(UiTheme.CardBorder, 1))
        {
            g.FillPath(fill, path);
            g.DrawPath(edge, path);
        }
    }

    public static void PaintMetricTile(Panel tile, PaintEventArgs e, Color accent)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, tile.Width - 1, tile.Height - 1);
        const int r = 14;
        using var path = CreateRoundRectPath(rect, r);
        using (var fill = new SolidBrush(UiTheme.MetricTileBackground))
            g.FillPath(fill, path);
        using (var border = new Pen(UiTheme.MetricTileBorder, 1))
            g.DrawPath(border, path);

        using var stripe = new Pen(accent, 3.5f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };
        g.DrawLine(stripe, 12, 16, 12, tile.Height - 16);
    }

    public static void PaintRoundedBanner(Panel panel, PaintEventArgs e, UiFeedbackKind kind)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
        const int r = 12;
        using var path = CreateRoundRectPath(rect, r);
        Color fill = kind switch
        {
            UiFeedbackKind.Error => UiTheme.ErrorBackground,
            UiFeedbackKind.Success => UiTheme.SuccessBackground,
            _ => Color.FromArgb(248, 252, 250)
        };
        Color edge = kind switch
        {
            UiFeedbackKind.Error => UiTheme.ErrorBorder,
            UiFeedbackKind.Success => UiTheme.SuccessBorder,
            _ => UiTheme.CardBorder
        };
        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);
        using (var pen = new Pen(edge, 1))
            g.DrawPath(pen, path);
    }
}
