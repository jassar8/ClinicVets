using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>Rounded border around a flat <see cref="ComboBox"/>.</summary>
public sealed class RoundedComboHost : Panel
{
    private bool _focused;

    public RoundedComboHost(ComboBox inner)
    {
        Inner = inner;
        Height = UiTheme.InputHeight;
        Margin = new Padding(0, 0, 0, 6);
        BackColor = Color.Transparent;
        inner.FlatStyle = FlatStyle.Flat;
        inner.Font = UiStyles.InputFont;
        inner.BackColor = UiTheme.InputBackground;
        inner.ForeColor = UiTheme.TextDark;
        inner.DropDownStyle = ComboBoxStyle.DropDownList;
        inner.TabStop = true;
        inner.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        inner.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        Controls.Add(inner);
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
        UpdateStyles();
    }

    public ComboBox Inner { get; }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        var padH = UiTheme.InputPaddingH;
        var innerH = Math.Max(28, ClientSize.Height - 8);
        Inner.SetBounds(padH - 2, (ClientSize.Height - innerH) / 2, Math.Max(40, ClientSize.Width - padH * 2 + 4), innerH);
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
