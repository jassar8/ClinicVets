namespace ClinicVets.Desktop.UI;

public enum UiFeedbackKind
{
    None,
    Error,
    Success
}

/// <summary>Typography shared across pages.</summary>
public static class UiStyles
{
    public static Font HeroTitleFont { get; } = new("Segoe UI", 28F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font HeroSubtitleFont { get; } = new("Segoe UI", 15.5F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font FieldCaptionFont { get; } = new("Segoe UI", 11.5F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font InputFont { get; } = new("Segoe UI", 14.5F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font PrimaryButtonFont { get; } = new("Segoe UI", 14.5F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font SecondaryButtonFont { get; } = new("Segoe UI", 14.5F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font DangerButtonFont { get; } = new("Segoe UI", 14.25F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font SidebarNavFont { get; } = new("Segoe UI", 12F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font SidebarNavFontActive { get; } = new("Segoe UI", 12.25F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font FeedbackFont { get; } = new("Segoe UI", 11F, FontStyle.Regular, GraphicsUnit.Point);

    public static void ApplyComboInner(ComboBox combo)
    {
        combo.FlatStyle = FlatStyle.Flat;
        combo.BackColor = UiTheme.InputBackground;
        combo.ForeColor = UiTheme.TextDark;
        combo.Font = InputFont;
    }

    public static Label CreateFieldCaption(string text) =>
        new()
        {
            Text = text,
            ForeColor = UiTheme.TextDark,
            Font = FieldCaptionFont,
            AutoSize = true,
            Margin = new Padding(0, 16, 0, 6)
        };

    public static Label CreateHeroTitle(string text) =>
        new()
        {
            Text = text,
            Font = HeroTitleFont,
            ForeColor = UiTheme.TextDark,
            TextAlign = ContentAlignment.TopCenter,
            AutoSize = true,
            Margin = new Padding(0, 6, 0, 4),
            UseCompatibleTextRendering = false
        };

    public static Label CreateHeroSubtitle(string text) =>
        new()
        {
            Text = text,
            Font = HeroSubtitleFont,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.TopCenter,
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 20),
            UseCompatibleTextRendering = false
        };
}
