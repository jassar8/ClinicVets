using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Owner-drawn primary action: deep teal, white label, rounded corners, hover/press/disabled states.</summary>
public sealed class ModernPrimaryButton : Button
{
    private bool _hover;
    private bool _pressed;
    private Color? _accentOverride;

    public ModernPrimaryButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        ForeColor = UiTheme.PrimaryButtonText;
        Font = UiStyles.PrimaryButtonFont;
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

    /// <summary>When set, replaces the default primary fill and derives hover/press shades for this button only.</summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color? AccentOverride
    {
        get => _accentOverride;
        set
        {
            if (_accentOverride == value)
                return;
            _accentOverride = value;
            Invalidate();
        }
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

    private static Color Shift(Color c, int delta) =>
        Color.FromArgb(
            c.A,
            Math.Clamp(c.R + delta, 0, 255),
            Math.Clamp(c.G + delta, 0, 255),
            Math.Clamp(c.B + delta, 0, 255));

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
        else if (AccentOverride is Color accent)
        {
            if (_pressed)
            {
                fill = Shift(accent, -22);
                textColor = UiTheme.PrimaryButtonText;
            }
            else if (_hover)
            {
                fill = Shift(accent, 16);
                textColor = UiTheme.PrimaryButtonText;
            }
            else
            {
                fill = accent;
                textColor = UiTheme.PrimaryButtonText;
            }
        }
        else if (_pressed)
        {
            fill = UiTheme.PrimaryButtonPressed;
            textColor = UiTheme.PrimaryButtonText;
        }
        else if (_hover)
        {
            fill = UiTheme.PrimaryButtonHover;
            textColor = UiTheme.PrimaryButtonText;
        }
        else
        {
            fill = UiTheme.PrimaryButton;
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
        // Fully custom chrome; avoid default button chrome/borders.
    }
}
