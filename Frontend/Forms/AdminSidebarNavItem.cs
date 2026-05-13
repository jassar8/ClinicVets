using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using ClinicVets.Desktop.UI;

namespace ClinicVets.Desktop.Forms;

/// <summary>Dark-sidebar navigation row with optional numeric badge.</summary>
internal sealed class AdminSidebarNavItem : Panel
{
    private readonly string _caption;
    private bool _hover;

    public AdminSidebarNavItem(string caption, AdminNavKind kind)
    {
        _caption = caption;
        Kind = kind;
        Height = 44;
        Cursor = Cursors.Hand;
        TabStop = false;
        BackColor = UiTheme.AdminSidebarBackground;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
        MouseEnter += (_, _) =>
        {
            _hover = true;
            Invalidate();
        };
        MouseLeave += (_, _) =>
        {
            _hover = false;
            Invalidate();
        };
    }

    public AdminNavKind Kind { get; }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int? BadgeCount { get; set; }

    private bool _active;

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool IsActive
    {
        get => _active;
        set
        {
            if (_active == value)
                return;
            _active = value;
            Invalidate();
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        const int r = 10;
        using var path = UiChrome.CreateRoundRectPath(rect, r);

        Color fill;
        if (IsActive)
            fill = UiTheme.AdminSidebarNavActive;
        else if (_hover)
            fill = UiTheme.AdminSidebarNavHover;
        else
            fill = UiTheme.AdminSidebarBackground;

        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);

        var textRect = new Rectangle(16, 0, Width - 56, Height);
        TextRenderer.DrawText(
            g,
            _caption,
            IsActive ? UiStyles.SidebarNavFontActive : UiStyles.SidebarNavFont,
            textRect,
            Color.White,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);

        if (BadgeCount is > 0)
        {
            var badgeText = BadgeCount > 99 ? "99+" : BadgeCount.Value.ToString();
            using var font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point);
            var sz = TextRenderer.MeasureText(badgeText, font);
            var badgeW = Math.Max(22, sz.Width + 10);
            var badgeRect = new Rectangle(Width - badgeW - 12, (Height - 20) / 2, badgeW, 20);
            using var bp = UiChrome.CreateRoundRectPath(badgeRect, 10);
            using (var bb = new SolidBrush(Color.FromArgb(230, 120, 50)))
                g.FillPath(bb, bp);
            TextRenderer.DrawText(
                g,
                badgeText,
                font,
                badgeRect,
                Color.White,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }
}
