namespace ClinicVets.Desktop.UI;

/// <summary>2026 ClinicVets palette: teal (#0F766E), dark sidebar (#115E59), soft mint (#CCFBF1), neutral canvas (#F5F7F7).</summary>
public static class UiTheme
{
    // --- Core palette (hex references in comments) ---
    public static Color TealMain => Color.FromArgb(0x0F, 0x76, 0x6E); // #0F766E
    public static Color TealSidebarDark => Color.FromArgb(0x11, 0x5E, 0x59); // #115E59
    public static Color TealHover => Color.FromArgb(0x0D, 0x5E, 0x58); // #0D5E58
    public static Color MintAccent => Color.FromArgb(0xCC, 0xFB, 0xF1); // #CCFBF1
    public static Color CanvasBackground => Color.FromArgb(0xF5, 0xF7, 0xF7); // #F5F7F7
    public static Color CharcoalText => Color.FromArgb(0x1F, 0x29, 0x37); // #1F2937
    public static Color SuccessGreen => Color.FromArgb(0x22, 0xC5, 0x5E); // #22C55E
    public static Color ErrorRed => Color.FromArgb(0xEF, 0x44, 0x44); // #EF4444
    public static Color WarningAmber => Color.FromArgb(0xF5, 0x9E, 0x0B); // #F59E0B

    public static Color PageBackground => CanvasBackground;
    public static Color PageGradientBottom => Color.FromArgb(0xEC, 0xF5, 0xF3);
    public static Color ContentCanvas => CanvasBackground;

    /// <summary>Login/register hero strip and primary brand headers.</summary>
    public static Color HeaderPrimary => TealMain;
    public static Color HeaderPrimaryDark => TealHover;

    public static Color AccentStrip => Color.FromArgb(0x2D, 0x9D, 0x91);
    public static Color AccentMintSoft => MintAccent;
    public static Color AccentMintWash => Color.FromArgb(0xE6, 0xFC, 0xF7);

    public static Color CardWhite => Color.FromArgb(255, 255, 255);
    public static Color CardBorder => Color.FromArgb(0xE2, 0xE8, 0xE6);
    public static int CardCornerRadius => 22;

    public static Color MetricTileBackground => CardWhite;
    public static Color MetricTileBorder => Color.FromArgb(0xD8, 0xE8, 0xE5);
    public static Color MetricAccentStripe => TealMain;

    public static Color TextMuted => Color.FromArgb(0x6B, 0x72, 0x7A);
    public static Color TextDark => CharcoalText;

    public static Color ErrorText => ErrorRed;
    public static Color ErrorBackground => Color.FromArgb(0xFE, 0xF2, 0xF2);
    public static Color ErrorBorder => Color.FromArgb(0xFE, 0xCA, 0xCA);

    public static Color SuccessText => Color.FromArgb(0x16, 0x65, 0x34);
    public static Color SuccessBackground => Color.FromArgb(0xEC, 0xFD, 0xF5);
    public static Color SuccessBorder => Color.FromArgb(0xBB, 0xF7, 0xD0);

    public static Color WarningText => Color.FromArgb(0x92, 0x4D, 0x0E);
    public static Color WarningBackground => Color.FromArgb(0xFF, 0xFB, 0xEB);
    public static Color WarningBorder => Color.FromArgb(0xFD, 0xE6, 0x8A);

    public static Color SubtitleOnHeader => MintAccent;

    public static Color InputBackground => CardWhite;
    public static Color InputBorder => Color.FromArgb(0xD1, 0xDB, 0xDA);
    public static Color InputBorderFocus => TealMain;
    public static int InputHeight => 50;
    public static int InputRadius => 14;
    public static int InputPaddingH => 14;

    public const int StandardButtonHeight = 48;
    public static int PrimaryButtonHeight => StandardButtonHeight;
    public static int SecondaryButtonHeight => StandardButtonHeight;
    public static int ButtonCornerRadius => 16;

    public static Color PrimaryButton => TealMain;
    public static Color PrimaryButtonHover => TealHover;
    public static Color PrimaryButtonPressed => Color.FromArgb(0x0A, 0x4E, 0x48);

    public static Color SecondaryButtonBackground => Color.White;
    public static Color SecondaryButtonBorder => TealMain;
    public static Color SecondaryButtonText => TealSidebarDark;
    public static Color SecondaryButtonHover => AccentMintWash;
    public static Color SecondaryButtonPressed => Color.FromArgb(0xD8, 0xF3, 0xED);

    public static Color DangerButton => ErrorRed;
    public static Color DangerButtonHover => Color.FromArgb(0xDC, 0x26, 0x26);
    public static Color DangerButtonPressed => Color.FromArgb(0xB9, 0x1C, 0x1C);

    public static Color ButtonDisabledFill => Color.FromArgb(0xF0, 0xF2, 0xF2);
    public static Color ButtonDisabledText => Color.FromArgb(0x9C, 0xA8, 0xAE);
    public static Color ButtonDisabledBorder => Color.FromArgb(0xE0, 0xE5, 0xE4);

    /// <summary>Staff shell sidebar (same dark treatment as admin for one cohesive product).</summary>
    public static Color SidebarBackground => TealSidebarDark;
    public static Color SidebarBorder => Color.FromArgb(0x0E, 0x4F, 0x4A);
    public static Color SidebarNavHover => TealHover;
    public static Color SidebarNavActive => TealMain;
    public static Color SidebarNavActiveBorder => MintAccent;
    public static Color SidebarNavText => Color.FromArgb(0xCC, 0xE8, 0xE4);
    public static Color SidebarNavTextActive => Color.White;
    public static Color SidebarNavTextHover => Color.White;
    public static Color SidebarMuted => SidebarNavText;
    public static Color SidebarItemHover => SidebarNavHover;
    public static Color SidebarItemActive => SidebarNavActive;
    public static int SidebarWidth => 272;

    public static Color HeaderBlue => HeaderPrimary;
    public static Color HeaderBlueDark => HeaderPrimaryDark;

    public static Color PrimaryButtonText => Color.White;

    /// <summary>Dark teal shell rail (matches sidebar spec).</summary>
    public static Color AdminSidebarBackground => TealSidebarDark;
    public static Color AdminSidebarNavActive => TealMain;
    public static Color AdminSidebarNavHover => TealHover;

    /// <summary>Muted line on dark sidebar (subtitle, dividers).</summary>
    public static Color SidebarTextMutedOnDark => Color.FromArgb(0xA7, 0xD4, 0xCF);

    public static Color SidebarLogoutBackground => TealHover;
    public static Color SidebarLogoutBorder => Color.FromArgb(0x80, 0xCC, 0xFB, 0xF1);

    /// <summary>Metric tile accent for pending / warning-style KPIs.</summary>
    public static Color MetricAccentPending => WarningAmber;

    public static Color MetricAccentSuccess => SuccessGreen;
    public static Color MetricAccentDanger => ErrorRed;

    /// <summary>Soft destructive fill (grid delete pill, etc.).</summary>
    public static Color ActionSoftDeleteFill => Color.FromArgb(0xFC, 0xA5, 0xA5);

    /// <summary>Soft primary-tint row (e.g. Review action).</summary>
    public static Color ActionReviewFill => AccentMintWash;

    public static Color GridHeaderBackground => AccentMintWash;
    public static Color GridHeaderForeColor => TealSidebarDark;
    public static Color GridSelectionBackground => Color.FromArgb(0xD9, 0xF7, 0xF0);
    public static Color GridRoleSecretaryTint => Color.FromArgb(0xEC, 0xF4, 0xFF);
    public static Color GridRoleVetTint => Color.FromArgb(0xF5, 0xF0, 0xFF);
    public static Color GridRoleAdminTint => Color.FromArgb(0xFF, 0xFB, 0xEB);
    public static Color GridPendingFore => WarningAmber;
    public static Color GridNeutralFill => Color.FromArgb(0xF3, 0xF4, 0xF6);

    public static Color DemoStripBackground => AccentMintWash;
    public static Color OverlayScrim => Color.FromArgb(0xC8, 0xF5, 0xF7, 0xF7);

    /// <summary>Soft teal-tinted drop shadow under elevated cards.</summary>
    public static Color CardShadowTint => Color.FromArgb(18, 15, 94, 89);

    /// <summary>Neutral feedback / info banner surface.</summary>
    public static Color InfoBannerBackground => AccentMintWash;

    /// <summary>Shared layout rhythm (px at 96 DPI; scales with <see cref="Application.SetHighDpiMode"/>).</summary>
    public static class Layout
    {
        public const int PageGutter = 16;
        public const int CardInset = 16;
        public const int SectionGap = 12;
        public const int HeaderMinHeight = 88;
        public const int SidebarMinWidth = 248;
        public const int SidebarMaxWidth = 288;
    }
}
