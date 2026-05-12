using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Outlined secondary action with rounded corners.</summary>
public sealed class ModernOutlineButton : Button
{
    private bool _hover;

    public ModernOutlineButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = UiTheme.TextDark;
        Font = new Font("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
        BackColor = Color.Transparent;
        Cursor = Cursors.Hand;
        Height = UiTheme.SecondaryButtonHeight;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        TabStop = true;
        MouseEnter += (_, _) => { _hover = true; Invalidate(); };
        MouseLeave += (_, _) => { _hover = false; Invalidate(); };
    }

    private bool _pressed;

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        _pressed = true;
        base.OnMouseDown(mevent);
        Invalidate();
    }

    protected override void OnMouseUp(MouseEventArgs mevent)
    {
        _pressed = false;
        base.OnMouseUp(mevent);
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = UiChrome.CreateRoundRectPath(rect, UiTheme.ButtonCornerRadius);
        Color fill = _pressed ? UiTheme.SecondaryButtonPressed : _hover ? UiTheme.SecondaryButtonHover : UiTheme.SecondaryButtonBackground;
        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);
        using var pen = new Pen(UiTheme.SecondaryButtonBorder, 1.25f);
        g.DrawPath(pen, path);

        using var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        using var brush = new SolidBrush(ForeColor);
        g.DrawString(Text, Font, brush, ClientRectangle, format);
    }
}
