using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Secondary action: white fill, teal border and text, mint hover wash.</summary>
public sealed class ModernOutlineButton : Button
{
    private bool _hover;
    private bool _pressed;

    public ModernOutlineButton()
    {
        MinimumSize = new Size(120, UiTheme.SecondaryButtonHeight);
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = UiTheme.SecondaryButtonText;
        Font = UiStyles.SecondaryButtonFont;
        BackColor = UiTheme.SecondaryButtonBackground;
        Cursor = Cursors.Hand;
        Height = UiTheme.SecondaryButtonHeight;
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

        UiButtonLayout.ApplyMinimumWidthForText(this);
    }

    protected override void OnTextChanged(EventArgs e)
    {
        base.OnTextChanged(e);
        UiButtonLayout.ApplyMinimumWidthForText(this);
    }

    protected override void OnFontChanged(EventArgs e)
    {
        base.OnFontChanged(e);
        UiButtonLayout.ApplyMinimumWidthForText(this);
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

        ForeColor = Enabled ? UiTheme.SecondaryButtonText : UiTheme.ButtonDisabledText;
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
        Color border;
        Color textColor;
        if (!Enabled)
        {
            fill = UiTheme.ButtonDisabledFill;
            border = UiTheme.ButtonDisabledBorder;
            textColor = UiTheme.ButtonDisabledText;
        }
        else if (_pressed)
        {
            fill = UiTheme.SecondaryButtonPressed;
            border = UiTheme.SecondaryButtonBorder;
            textColor = UiTheme.SecondaryButtonText;
        }
        else if (_hover)
        {
            fill = UiTheme.SecondaryButtonHover;
            border = UiTheme.SecondaryButtonBorder;
            textColor = UiTheme.SecondaryButtonText;
        }
        else
        {
            fill = UiTheme.SecondaryButtonBackground;
            border = UiTheme.SecondaryButtonBorder;
            textColor = UiTheme.SecondaryButtonText;
        }

        using (var b = new SolidBrush(fill))
            g.FillPath(b, path);

        using var pen = new Pen(border, 1f);
        g.DrawPath(pen, path);

        g.SetClip(path);
        using var format = new StringFormat
        {
            Alignment = StringAlignment.Center,
            LineAlignment = StringAlignment.Center,
            Trimming = StringTrimming.EllipsisCharacter,
            FormatFlags = StringFormatFlags.NoWrap | StringFormatFlags.LineLimit
        };
        using var brush = new SolidBrush(textColor);
        var textRect = ClientRectangle;
        textRect.Inflate(-18, -10);
        g.DrawString(Text, Font, brush, textRect, format);
        g.ResetClip();
    }

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }
}
