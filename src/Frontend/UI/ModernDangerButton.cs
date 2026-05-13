using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Destructive actions: soft red fill, white label, rounded corners.</summary>
public sealed class ModernDangerButton : Button
{
    private bool _hover;
    private bool _pressed;

    public ModernDangerButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = UiTheme.PrimaryButtonText;
        Font = UiStyles.DangerButtonFont;
        Cursor = Cursors.Hand;
        Height = UiTheme.PrimaryButtonHeight;
        TabStop = true;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();

        MouseEnter += (_, _) =>
        {
            if (!Enabled)
                return;
            _hover = true;
            Invalidate();
        };
        MouseLeave += (_, _) =>
        {
            _hover = false;
            Invalidate();
        };
    }

    protected override void OnEnabledChanged(EventArgs e)
    {
        if (!Enabled)
        {
            _hover = false;
            _pressed = false;
            Cursor = Cursors.Default;
        }
        else
        {
            Cursor = Cursors.Hand;
        }

        base.OnEnabledChanged(e);
        Invalidate();
    }

    protected override void OnLostFocus(EventArgs e)
    {
        _pressed = false;
        base.OnLostFocus(e);
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs mevent)
    {
        if (Enabled && mevent.Button == MouseButtons.Left)
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
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        var radius = Math.Min(UiTheme.ButtonCornerRadius, Math.Min(rect.Width, rect.Height) / 2);
        using var path = UiChrome.CreateRoundRectPath(rect, radius);

        Color fill;
        Color textColor;
        if (!Enabled)
        {
            fill = UiTheme.ButtonDisabledFill;
            textColor = UiTheme.ButtonDisabledText;
        }
        else if (_pressed)
        {
            fill = UiTheme.DangerButtonPressed;
            textColor = UiTheme.PrimaryButtonText;
        }
        else if (_hover)
        {
            fill = UiTheme.DangerButtonHover;
            textColor = UiTheme.PrimaryButtonText;
        }
        else
        {
            fill = UiTheme.DangerButton;
            textColor = UiTheme.PrimaryButtonText;
        }

        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);

        g.SetClip(path);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap
        };
        using var brush = new SolidBrush(textColor);
        var textRect = ClientRectangle;
        textRect.Inflate(-14, -8);
        g.DrawString(Text, Font, brush, textRect, format);
        g.ResetClip();
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }
}
