namespace ClinicVets.Desktop.UI;

/// <summary>
/// Shared WinForms colors for a consistent demo presentation.
/// </summary>
public static class UiTheme
{
    public static Color PageBackground => Color.FromArgb(243, 247, 251);
    public static Color HeaderBlue => Color.FromArgb(30, 95, 164);
    public static Color HeaderBlueDark => Color.FromArgb(24, 79, 132);
    public static Color CardWhite => Color.White;
    public static Color CardBorder => Color.FromArgb(210, 222, 236);
    public static int CardCornerRadius => 14;
    public static Color MetricTileBackground => Color.FromArgb(247, 251, 255);
    public static Color TextMuted => Color.FromArgb(90, 110, 130);
    public static Color TextDark => Color.FromArgb(38, 54, 74);
    public static Color ErrorText => Color.FromArgb(180, 40, 40);
    public static Color SubtitleOnHeader => Color.FromArgb(220, 235, 255);

    public static Color InputBackground => Color.White;
    public static Color InputBorder => Color.FromArgb(200, 214, 230);
    public static int InputHeight => 42;
    public static int PrimaryButtonHeight => 46;
    public static int SecondaryButtonHeight => 46;

    public static Color PrimaryButtonHover => Color.FromArgb(38, 110, 188);
    public static Color SecondaryButtonBackground => Color.White;
    public static Color SecondaryButtonBorder => Color.FromArgb(200, 214, 230);
    public static Color SecondaryButtonHover => Color.FromArgb(248, 250, 253);
    public static Color SecondaryButtonPressed => Color.FromArgb(236, 242, 250);
}
