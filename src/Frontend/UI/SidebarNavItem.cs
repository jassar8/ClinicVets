using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using ClinicVets.Application.Security;

namespace ClinicVets.Desktop.UI;

/// <summary>Rounded sidebar navigation row with hover and active (pill) states.</summary>
public sealed class SidebarNavItem : Panel
{
    private readonly string _caption;
    private bool _hover;

    public SidebarNavItem(DashboardSection section, string caption)
    {
        Section = section;
        _caption = caption;
        Height = 44;
        Cursor = Cursors.Hand;
        BackColor = UiTheme.SidebarBackground;
        TabStop = false;
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

    public DashboardSection Section { get; }

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
        const int radius = 12;
        using var path = UiChrome.CreateRoundRectPath(rect, radius);

        Color fill;
        if (IsActive)
            fill = UiTheme.SidebarNavActive;
        else if (_hover)
            fill = UiTheme.SidebarNavHover;
        else
            fill = UiTheme.SidebarBackground;

        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);

        if (IsActive)
        {
            using var edge = new Pen(UiTheme.SidebarNavActiveBorder, 1f);
            g.DrawPath(edge, path);
        }

        var textColor = IsActive ? UiTheme.SidebarNavTextActive : _hover ? UiTheme.SidebarNavTextHover : UiTheme.SidebarNavText;
        var font = IsActive ? UiStyles.SidebarNavFontActive : UiStyles.SidebarNavFont;
        var pad = new Rectangle(16, 0, Math.Max(0, Width - 20), Height);
        TextRenderer.DrawText(
            g,
            _caption,
            font,
            pad,
            textColor,
            TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }
}
