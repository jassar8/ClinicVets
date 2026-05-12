namespace ClinicVets.Desktop.UI;

/// <summary>
/// Applies consistent control styling (aligned with page-log-in typography and rounded controls).
/// </summary>
public static class UiStyles
{
    public static Font HeroTitleFont { get; } = new("Segoe UI", 26F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font HeroSubtitleFont { get; } = new("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font FieldCaptionFont { get; } = new("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font InputFont { get; } = new("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font PrimaryButtonFont { get; } = new("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font SecondaryButtonFont { get; } = new("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);

    public static void ApplyTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = UiTheme.InputBackground;
        textBox.ForeColor = UiTheme.TextDark;
        textBox.Font = InputFont;
        textBox.Height = Math.Max(textBox.Height, UiTheme.InputHeight);
    }

    public static void ApplyComboBox(ComboBox combo)
    {
        combo.FlatStyle = FlatStyle.Flat;
        combo.BackColor = UiTheme.InputBackground;
        combo.ForeColor = UiTheme.TextDark;
        combo.Font = InputFont;
        combo.Height = Math.Max(combo.Height, UiTheme.InputHeight);
    }

    public static void ApplyPrimaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = UiTheme.HeaderBlue;
        button.ForeColor = Color.White;
        button.Font = PrimaryButtonFont;
        button.Cursor = Cursors.Hand;
        button.Height = Math.Max(button.Height, UiTheme.PrimaryButtonHeight);
        button.FlatAppearance.MouseOverBackColor = UiTheme.PrimaryButtonHover;
        button.FlatAppearance.MouseDownBackColor = UiTheme.HeaderBlueDark;
    }

    public static void ApplySecondaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 1;
        button.FlatAppearance.BorderColor = UiTheme.SecondaryButtonBorder;
        button.BackColor = UiTheme.SecondaryButtonBackground;
        button.ForeColor = UiTheme.TextDark;
        button.Font = SecondaryButtonFont;
        button.Cursor = Cursors.Hand;
        button.Height = Math.Max(button.Height, UiTheme.SecondaryButtonHeight);
        button.FlatAppearance.MouseOverBackColor = UiTheme.SecondaryButtonHover;
        button.FlatAppearance.MouseDownBackColor = UiTheme.SecondaryButtonPressed;
    }

    public static Label CreateFieldCaption(string text) =>
        new()
        {
            Text = text,
            ForeColor = UiTheme.TextDark,
            Font = FieldCaptionFont,
            AutoSize = true,
            Margin = new Padding(0, 12, 0, 6)
        };

    /// <summary>Centered block title inside a card (page-log-in style product heading).</summary>
    public static Label CreateHeroTitle(string text)
    {
        var l = new Label
        {
            Text = text,
            Font = HeroTitleFont,
            ForeColor = UiTheme.TextDark,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Height = 52,
            Margin = new Padding(0, 0, 0, 4)
        };
        return l;
    }

    public static Label CreateHeroSubtitle(string text) =>
        new()
        {
            Text = text,
            Font = HeroSubtitleFont,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Height = 28,
            Margin = new Padding(0, 0, 0, 20)
        };
}
