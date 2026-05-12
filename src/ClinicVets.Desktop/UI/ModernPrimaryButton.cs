using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Owner-drawn primary action with rounded corners and hover states.</summary>
public sealed class ModernPrimaryButton : Button
{
    private bool _hover;

    public ModernPrimaryButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = Color.White;
        Font = new Font("Segoe UI", 15F, FontStyle.Bold, GraphicsUnit.Point);
        Cursor = Cursors.Hand;
        Height = UiTheme.PrimaryButtonHeight;
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
        Color fill;
        if (!Enabled)
            fill = Color.FromArgb(160, 180, 176);
        else if (_pressed)
            fill = UiTheme.PrimaryButtonPressed;
        else if (_hover)
            fill = UiTheme.PrimaryButtonHover;
        else
            fill = UiTheme.PrimaryButton;

        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);

        using (var format = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
        using (var brush = new SolidBrush(ForeColor))
            g.DrawString(Text, Font, brush, ClientRectangle, format);
    }
}
