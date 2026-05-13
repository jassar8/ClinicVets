using System.ComponentModel;
using System.Drawing.Drawing2D;

namespace ClinicVets.Desktop.UI;

/// <summary>White rounded card with soft teal shadow (WPF-style shell surface).</summary>
public sealed class ModernCardPanel : Panel
{
    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int CornerRadius { get; set; } = UiTheme.CardCornerRadius;

    public ModernCardPanel()
    {
        BackColor = Color.Transparent;
        SetStyle(
            ControlStyles.AllPaintingInWmPaint |
            ControlStyles.UserPaint |
            ControlStyles.OptimizedDoubleBuffer |
            ControlStyles.ResizeRedraw,
            true);
        UpdateStyles();
    }

    protected override void OnPaint(PaintEventArgs e) =>
        UiChrome.PaintCardWithShadow(this, e, CornerRadius);

    protected override void OnPaintBackground(PaintEventArgs pevent)
    {
    }
}
