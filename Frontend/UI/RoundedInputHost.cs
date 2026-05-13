using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Rounded field chrome around a borderless <see cref="TextBox"/>.</summary>
public sealed class RoundedInputHost : Panel
{
    private bool _focused;
    private readonly PasswordRevealToggle? _revealToggle;

    public RoundedInputHost(TextBox inner, bool showPasswordRevealToggle = false)
    {
        Inner = inner;
        Height = UiTheme.InputHeight;
        Margin = new Padding(0, 0, 0, 6);
        BackColor = Color.Transparent;
        inner.BorderStyle = BorderStyle.None;
        inner.Font = UiStyles.InputFont;
        inner.BackColor = UiTheme.InputBackground;
        inner.ForeColor = UiTheme.TextDark;
        inner.TabStop = true;
        inner.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        inner.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        Controls.Add(inner);

        if (showPasswordRevealToggle)
        {
            _revealToggle = new PasswordRevealToggle(inner);
            Controls.Add(_revealToggle);
        }

        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        UpdateStyles();
    }

    public TextBox Inner { get; }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        var padH = UiTheme.InputPaddingH;
        var innerH = Math.Max(26, ClientSize.Height - 10);
        var revealW = _revealToggle?.Width ?? 0;
        var gap = revealW > 0 ? 6 : 0;
        Inner.SetBounds(
            padH,
            (ClientSize.Height - innerH) / 2,
            Math.Max(40, ClientSize.Width - padH * 2 - revealW - gap),
            innerH);

        if (_revealToggle is not null)
        {
            var h = Math.Max(28, ClientSize.Height - 8);
            _revealToggle.SetBounds(ClientSize.Width - padH - _revealToggle.Width, (ClientSize.Height - h) / 2, _revealToggle.Width, h);
        }
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = UiChrome.CreateRoundRectPath(rect, UiTheme.InputRadius);
        using (var fill = new SolidBrush(UiTheme.InputBackground))
            g.FillPath(fill, path);
        var edge = _focused ? UiTheme.InputBorderFocus : UiTheme.InputBorder;
        using var pen = new Pen(edge, _focused ? 1.5f : 1f);
        g.DrawPath(pen, path);
    }
}
