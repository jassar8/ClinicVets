using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>
/// Minimal eye control for toggling <see cref="TextBox.UseSystemPasswordChar"/> (inside rounded password fields).
/// </summary>
public sealed class PasswordRevealToggle : Control
{
    private readonly TextBox _password;
    private bool _hover;
    private bool _pressed;

    public PasswordRevealToggle(TextBox password)
    {
        _password = password;
        TabStop = true;
        Cursor = Cursors.Hand;
        BackColor = Color.Transparent;
        Size = new Size(44, UiTheme.InputHeight);
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
        UpdateAccessibleDescription();
    }

    private bool PasswordHidden => _password.UseSystemPasswordChar;

    private void UpdateAccessibleDescription()
    {
        AccessibleRole = AccessibleRole.PushButton;
        AccessibleName = PasswordHidden ? "Show password" : "Hide password";
    }

    protected override void OnMouseEnter(EventArgs e)
    {
        base.OnMouseEnter(e);
        _hover = true;
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hover = false;
        _pressed = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _pressed = true;
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        _pressed = false;
        Invalidate();
    }

    protected override void OnClick(EventArgs e)
    {
        TogglePasswordVisibility();
        base.OnClick(e);
    }

    private void TogglePasswordVisibility()
    {
        _password.UseSystemPasswordChar = !_password.UseSystemPasswordChar;
        UpdateAccessibleDescription();
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        var cx = ClientSize.Width / 2f;
        var cy = ClientSize.Height / 2f;

        if (_hover || Focused)
        {
            using var halo = new SolidBrush(_pressed ? Color.FromArgb(48, 40, 138, 124) : Color.FromArgb(28, 40, 138, 124));
            var r = Math.Min(ClientSize.Width, ClientSize.Height) * 0.72f;
            g.FillEllipse(halo, cx - r / 2f, cy - r / 2f, r, r);
        }

        var ink = _hover || Focused ? UiTheme.PrimaryButton : UiTheme.TextMuted;
        using var line = new Pen(ink, 1.85f)
        {
            LineJoin = LineJoin.Round,
            StartCap = LineCap.Round,
            EndCap = LineCap.Round
        };

        var eyeW = 22f;
        var eyeH = 14f;
        var oval = new RectangleF(cx - eyeW / 2f, cy - eyeH / 2f, eyeW, eyeH);
        g.DrawEllipse(line, oval);

        using var pupil = new SolidBrush(ink);
        g.FillEllipse(pupil, cx - 2.5f, cy - 2.5f, 5f, 5f);

        // Slash when password is visible (plain text)
        if (!PasswordHidden)
        {
            var pad = 5f;
            g.DrawLine(
                line,
                oval.Left + pad,
                oval.Bottom - pad,
                oval.Right - pad,
                oval.Top + pad);
        }
    }

    protected override bool IsInputKey(Keys keyData) =>
        keyData is Keys.Space or Keys.Enter || base.IsInputKey(keyData);

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.KeyCode is Keys.Space or Keys.Enter)
        {
            TogglePasswordVisibility();
            e.Handled = true;
        }
    }
}
