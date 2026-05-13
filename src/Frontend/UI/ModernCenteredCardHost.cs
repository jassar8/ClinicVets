using System.ComponentModel;

namespace ClinicVets.Desktop.UI;

/// <summary>
/// Fills the host and keeps the first child centered horizontally with a clamped width
/// (responsive login/register cards without scattered manual <c>Left</c>/<c>Top</c> math).
/// </summary>
public sealed class ModernCenteredCardHost : Panel
{
    public ModernCenteredCardHost()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);
        UpdateStyles();
        BackColor = Color.Transparent;
    }

    [Browsable(true)]
    [DefaultValue(40)]
    public int HorizontalPadding { get; set; } = 40;

    [Browsable(true)]
    [DefaultValue(28)]
    public int VerticalPadding { get; set; } = 28;

    [Browsable(true)]
    [DefaultValue(520)]
    public int MaxContentWidth { get; set; } = 520;

    [Browsable(true)]
    [DefaultValue(300)]
    public int MinContentWidth { get; set; } = 300;

    protected override void OnLayout(LayoutEventArgs levent)
    {
        base.OnLayout(levent);
        if (Controls.Count == 0)
            return;

        var child = Controls[0];
        var innerW = ClientSize.Width - HorizontalPadding * 2;
        if (innerW < 1)
            return;

        var w = Math.Clamp(innerW, MinContentWidth, MaxContentWidth);
        var x = Math.Max(0, (ClientSize.Width - w) / 2);
        var y = VerticalPadding;
        var h = Math.Max(1, ClientSize.Height - VerticalPadding * 2);
        child.SetBounds(x, y, w, h);
    }
}
