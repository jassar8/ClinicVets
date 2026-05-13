using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ClinicVets.Desktop.UI;

/// <summary>Compact pill badge (counts, status chips).</summary>
public sealed class ModernBadgeLabel : Control
{
    private Color _back = UiTheme.WarningAmber;
    private Color _fore = Color.White;

    public ModernBadgeLabel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        UpdateStyles();
        Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
        ForeColor = _fore;
        BackColor = Color.Transparent;
        Padding = new Padding(10, 4, 10, 4);
        Size = new Size(40, 22);
    }

    public void SetColors(Color background, Color foreground)
    {
        _back = background;
        _fore = foreground;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = UiChrome.CreateRoundRectPath(rect, 10);
        using (var b = new SolidBrush(_back))
            g.FillPath(b, path);
        TextRenderer.DrawText(
            g,
            Text,
            Font,
            rect,
            _fore,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }
}
