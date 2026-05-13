namespace ClinicVets.Desktop.UI;

/// <summary>2026 veterinary clinic palette: deep teal, soft mint, light gray-green canvas, white cards.</summary>
public static class UiTheme
{
    public static Color PageBackground => Color.FromArgb(244, 249, 246);
    public static Color PageGradientBottom => Color.FromArgb(228, 240, 235);
    public static Color ContentCanvas => Color.FromArgb(236, 244, 240);

    public static Color HeaderPrimary => Color.FromArgb(0, 95, 95);
    public static Color HeaderPrimaryDark => Color.FromArgb(0, 72, 72);
    public static Color AccentStrip => Color.FromArgb(120, 210, 188);
    public static Color AccentMintSoft => Color.FromArgb(210, 244, 232);
    public static Color AccentMintWash => Color.FromArgb(232, 248, 242);

    public static Color CardWhite => Color.FromArgb(255, 255, 255);
    public static Color CardBorder => Color.FromArgb(214, 228, 220);
    public static int CardCornerRadius => 22;

    public static Color MetricTileBackground => Color.FromArgb(252, 254, 253);
    public static Color MetricTileBorder => Color.FromArgb(210, 224, 216);
    public static Color MetricAccentStripe => Color.FromArgb(52, 148, 128);

    public static Color TextMuted => Color.FromArgb(86, 102, 108);
    public static Color TextDark => Color.FromArgb(26, 38, 48);

    public static Color ErrorText => Color.FromArgb(168, 52, 52);
    public static Color ErrorBackground => Color.FromArgb(255, 246, 244);
    public static Color ErrorBorder => Color.FromArgb(236, 198, 194);

    public static Color SuccessText => Color.FromArgb(28, 108, 72);
    public static Color SuccessBackground => Color.FromArgb(228, 246, 236);
    public static Color SuccessBorder => Color.FromArgb(176, 222, 196);

    public static Color SubtitleOnHeader => Color.FromArgb(220, 244, 240);

    public static Color InputBackground => Color.FromArgb(255, 255, 255);
    public static Color InputBorder => Color.FromArgb(198, 216, 208);
    public static Color InputBorderFocus => Color.FromArgb(28, 132, 122);
    public static int InputHeight => 50;
    public static int InputRadius => 14;
    public static int InputPaddingH => 14;

    public const int StandardButtonHeight = 48;
    public static int PrimaryButtonHeight => StandardButtonHeight;
    public static int SecondaryButtonHeight => StandardButtonHeight;
    public static int ButtonCornerRadius => 16;

    /// <summary>Primary actions: deep teal fill.</summary>
    public static Color PrimaryButton => Color.FromArgb(18, 122, 116);

    public static Color PrimaryButtonHover => Color.FromArgb(22, 142, 132);
    public static Color PrimaryButtonPressed => Color.FromArgb(14, 98, 94);

    public static Color SecondaryButtonBackground => Color.White;
    public static Color SecondaryButtonBorder => Color.FromArgb(62, 148, 138);
    public static Color SecondaryButtonText => Color.FromArgb(14, 108, 102);
    public static Color SecondaryButtonHover => AccentMintWash;
    public static Color SecondaryButtonPressed => Color.FromArgb(218, 238, 230);

    public static Color DangerButton => Color.FromArgb(214, 82, 82);
    public static Color DangerButtonHover => Color.FromArgb(198, 68, 68);
    public static Color DangerButtonPressed => Color.FromArgb(176, 56, 56);

    public static Color ButtonDisabledFill => Color.FromArgb(228, 232, 230);
    public static Color ButtonDisabledText => Color.FromArgb(140, 152, 148);
    public static Color ButtonDisabledBorder => Color.FromArgb(200, 208, 204);

    public static Color SidebarBackground => Color.FromArgb(255, 255, 255);
    public static Color SidebarBorder => Color.FromArgb(216, 230, 222);
    public static Color SidebarNavHover => AccentMintWash;
    public static Color SidebarNavActive => Color.FromArgb(220, 242, 232);
    public static Color SidebarNavActiveBorder => Color.FromArgb(120, 200, 176);
    public static Color SidebarNavText => Color.FromArgb(100, 118, 114);
    public static Color SidebarNavTextActive => PrimaryButton;
    public static Color SidebarMuted => SidebarNavText;
    public static Color SidebarItemHover => SidebarNavHover;
    public static Color SidebarItemActive => SidebarNavActive;
    public static int SidebarWidth => 272;

    public static Color HeaderBlue => HeaderPrimary;
    public static Color HeaderBlueDark => HeaderPrimaryDark;

    public static Color PrimaryButtonText => Color.White;

    /// <summary>Administrator shell: deep teal sidebar (2026 dashboard).</summary>
    public static Color AdminSidebarBackground => Color.FromArgb(14, 72, 78);

    public static Color AdminSidebarNavActive => Color.FromArgb(28, 118, 110);
    public static Color AdminSidebarNavHover => Color.FromArgb(22, 96, 90);
}
