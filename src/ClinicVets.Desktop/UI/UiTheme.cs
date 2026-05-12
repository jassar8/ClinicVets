namespace ClinicVets.Desktop.UI;

/// <summary>
/// Calm clinic / veterinary palette — professional academic demo tone.
/// </summary>
public static class UiTheme
{
    /// <summary>Soft off-white with a hint of mint (easy on the eyes).</summary>
    public static Color PageBackground => Color.FromArgb(246, 250, 248);

    /// <summary>Deep teal — primary brand bar (trust, clinical calm).</summary>
    public static Color HeaderPrimary => Color.FromArgb(38, 108, 118);

    public static Color HeaderPrimaryDark => Color.FromArgb(28, 86, 94);
    public static Color AccentStrip => Color.FromArgb(92, 158, 132);

    public static Color CardWhite => Color.FromArgb(255, 255, 255);
    public static Color CardBorder => Color.FromArgb(208, 224, 218);
    public static int CardCornerRadius => 16;

    public static Color MetricTileBackground => Color.FromArgb(252, 254, 253);
    public static Color MetricTileBorder => Color.FromArgb(218, 232, 226);

    public static Color TextMuted => Color.FromArgb(88, 108, 102);
    public static Color TextDark => Color.FromArgb(34, 52, 56);

    public static Color ErrorText => Color.FromArgb(150, 42, 42);
    public static Color ErrorBackground => Color.FromArgb(255, 244, 242);
    public static Color ErrorBorder => Color.FromArgb(240, 200, 198);

    public static Color SuccessText => Color.FromArgb(42, 112, 78);
    public static Color SuccessBackground => Color.FromArgb(236, 248, 241);
    public static Color SuccessBorder => Color.FromArgb(188, 226, 204);

    public static Color SubtitleOnHeader => Color.FromArgb(220, 240, 242);

    public static Color InputBackground => Color.FromArgb(255, 255, 255);
    public static Color InputBorder => Color.FromArgb(198, 216, 210);
    public static int InputHeight => 44;
    public static int PrimaryButtonHeight => 48;
    public static int SecondaryButtonHeight => 48;

    public static Color PrimaryButton => Color.FromArgb(46, 124, 112);
    public static Color PrimaryButtonHover => Color.FromArgb(56, 142, 128);
    public static Color PrimaryButtonPressed => Color.FromArgb(36, 102, 92);

    public static Color SecondaryButtonBackground => Color.White;
    public static Color SecondaryButtonBorder => Color.FromArgb(198, 216, 210);
    public static Color SecondaryButtonHover => Color.FromArgb(244, 250, 248);
    public static Color SecondaryButtonPressed => Color.FromArgb(232, 244, 240);

    /// <summary>Legacy name — maps to header bar fill.</summary>
    public static Color HeaderBlue => HeaderPrimary;

    public static Color HeaderBlueDark => HeaderPrimaryDark;
}
