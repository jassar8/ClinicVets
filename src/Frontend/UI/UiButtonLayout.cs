using System.Drawing;
using System.Windows.Forms;

namespace ClinicVets.Desktop.UI;

/// <summary>Shared sizing so owner-drawn buttons never clip labels when the parent layout is tight.</summary>
public static class UiButtonLayout
{
    /// <summary>Horizontal padding included in <see cref="ApplyMinimumWidthForText"/> (left + right).</summary>
    public const int OwnerDrawHorizontalTextPadding = 40;

    /// <summary>Sets <see cref="Control.MinimumSize"/> width from measured text (single line).</summary>
    public static void ApplyMinimumWidthForText(Control control, int horizontalPadding = OwnerDrawHorizontalTextPadding, int floor = 120, int ceiling = 960)
    {
        var text = control.Text ?? string.Empty;
        if (string.IsNullOrEmpty(text))
            return;

        var w = TextRenderer.MeasureText(
                text,
                control.Font,
                new Size(int.MaxValue, int.MaxValue),
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding)
            .Width;

        var minW = Math.Clamp(w + horizontalPadding, floor, ceiling);
        var h = Math.Max(control.MinimumSize.Height, UiTheme.StandardButtonHeight);
        control.MinimumSize = new Size(minW, h);
    }
}
