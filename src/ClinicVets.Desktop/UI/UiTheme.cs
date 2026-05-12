namespace ClinicVets.Desktop.UI;

/// <summary>2026-style clinic palette: cool neutrals, teal, and sage accents.</summary>
public static class UiTheme
{
    public static Color PageBackground => Color.FromArgb(241, 247, 245);
    public static Color ContentCanvas => Color.FromArgb(236, 243, 241);

    public static Color HeaderPrimary => Color.FromArgb(32, 112, 118);
    public static Color HeaderPrimaryDark => Color.FromArgb(22, 88, 94);
    public static Color AccentStrip => Color.FromArgb(72, 168, 132);

    public static Color CardWhite => Color.FromArgb(255, 255, 255);
    public static Color CardBorder => Color.FromArgb(210, 228, 220);
    public static int CardCornerRadius => 20;

    public static Color MetricTileBackground => Color.FromArgb(252, 254, 253);
    public static Color MetricTileBorder => Color.FromArgb(214, 230, 222);

    public static Color TextMuted => Color.FromArgb(82, 102, 98);
    public static Color TextDark => Color.FromArgb(24, 44, 48);

    public static Color ErrorText => Color.FromArgb(142, 36, 36);
    public static Color ErrorBackground => Color.FromArgb(255, 246, 244);
    public static Color ErrorBorder => Color.FromArgb(235, 190, 186);

    public static Color SuccessText => Color.FromArgb(28, 108, 72);
    public static Color SuccessBackground => Color.FromArgb(232, 248, 238);
    public static Color SuccessBorder => Color.FromArgb(176, 222, 196);

    public static Color SubtitleOnHeader => Color.FromArgb(216, 242, 240);

    public static Color InputBackground => Color.FromArgb(255, 255, 255);
    public static Color InputBorder => Color.FromArgb(188, 210, 202);
    public static Color InputBorderFocus => Color.FromArgb(56, 150, 138);
    public static int InputHeight => 50;
    public static int InputRadius => 12;
    public static int InputPaddingH => 14;

    public static int PrimaryButtonHeight => 52;
    public static int SecondaryButtonHeight => 50;
    public static int ButtonCornerRadius => 14;

    public static Color PrimaryButton => Color.FromArgb(40, 138, 124);
    public static Color PrimaryButtonHover => Color.FromArgb(48, 156, 140);
    public static Color PrimaryButtonPressed => Color.FromArgb(30, 118, 106);

    public static Color SecondaryButtonBackground => Color.White;
    public static Color SecondaryButtonBorder => Color.FromArgb(188, 210, 202);
    public static Color SecondaryButtonHover => Color.FromArgb(244, 251, 248);
    public static Color SecondaryButtonPressed => Color.FromArgb(228, 240, 236);

    public static Color SidebarBackground => Color.FromArgb(255, 255, 255);
    public static Color SidebarBorder => Color.FromArgb(218, 232, 226);
    public static Color SidebarItemHover => Color.FromArgb(236, 246, 242);
    public static Color SidebarItemActive => Color.FromArgb(228, 244, 236);
    public static Color SidebarMuted => Color.FromArgb(130, 152, 146);
    public static int SidebarWidth => 272;

    public static Color HeaderBlue => HeaderPrimary;
    public static Color HeaderBlueDark => HeaderPrimaryDark;
}
