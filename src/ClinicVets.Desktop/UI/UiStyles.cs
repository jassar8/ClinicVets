namespace ClinicVets.Desktop.UI;

public enum UiFeedbackKind
{
    None,
    Error,
    Success
}

/// <summary>Typography and control chrome shared across auth and dashboard screens.</summary>
public static class UiStyles
{
    public static Font HeroTitleFont { get; } = new("Segoe UI", 26F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font HeroSubtitleFont { get; } = new("Segoe UI", 15F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font FieldCaptionFont { get; } = new("Segoe UI", 11F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font InputFont { get; } = new("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font PrimaryButtonFont { get; } = new("Segoe UI", 14F, FontStyle.Bold, GraphicsUnit.Point);
    public static Font SecondaryButtonFont { get; } = new("Segoe UI", 14F, FontStyle.Regular, GraphicsUnit.Point);
    public static Font FeedbackFont { get; } = new("Segoe UI", 10.5F, FontStyle.Regular, GraphicsUnit.Point);

    public static void ApplyTextBox(TextBox textBox)
    {
        textBox.BorderStyle = BorderStyle.FixedSingle;
        textBox.BackColor = UiTheme.InputBackground;
        textBox.ForeColor = UiTheme.TextDark;
        textBox.Font = InputFont;
        textBox.Height = Math.Max(textBox.Height, UiTheme.InputHeight);
        textBox.Margin = new Padding(0, 0, 0, 4);
    }

    public static void ApplyComboBox(ComboBox combo)
    {
        combo.FlatStyle = FlatStyle.Flat;
        combo.BackColor = UiTheme.InputBackground;
        combo.ForeColor = UiTheme.TextDark;
        combo.Font = InputFont;
        combo.Height = Math.Max(combo.Height, UiTheme.InputHeight);
        combo.Margin = new Padding(0, 0, 0, 4);
    }

    public static void ApplyPrimaryButton(Button button)
    {
        button.FlatStyle = FlatStyle.Flat;
        button.FlatAppearance.BorderSize = 0;
        button.BackColor = UiTheme.PrimaryButton;
        button.ForeColor = Color.White;
        button.Font = PrimaryButtonFont;
        button.Cursor = Cursors.Hand;
        button.Height = Math.Max(button.Height, UiTheme.PrimaryButtonHeight);
        button.FlatAppearance.MouseOverBackColor = UiTheme.PrimaryButtonHover;
        button.FlatAppearance.MouseDownBackColor = UiTheme.PrimaryButtonPressed;
        button.Margin = new Padding(0, 14, 0, 6);
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
        button.Margin = new Padding(0, 6, 0, 0);
    }

    public static void ApplyFeedbackLabel(Label label, UiFeedbackKind kind)
    {
        label.Font = FeedbackFont;
        label.AutoSize = false;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.Padding = new Padding(12, 10, 12, 10);
        switch (kind)
        {
            case UiFeedbackKind.Error:
                label.ForeColor = UiTheme.ErrorText;
                label.BackColor = UiTheme.ErrorBackground;
                label.BorderStyle = BorderStyle.FixedSingle;
                break;
            case UiFeedbackKind.Success:
                label.ForeColor = UiTheme.SuccessText;
                label.BackColor = UiTheme.SuccessBackground;
                label.BorderStyle = BorderStyle.FixedSingle;
                break;
            default:
                label.ForeColor = UiTheme.TextMuted;
                label.BackColor = Color.Transparent;
                label.BorderStyle = BorderStyle.None;
                break;
        }
    }

    public static Label CreateFieldCaption(string text) =>
        new()
        {
            Text = text,
            ForeColor = UiTheme.TextDark,
            Font = FieldCaptionFont,
            AutoSize = true,
            Margin = new Padding(0, 14, 0, 6)
        };

    public static Label CreateHeroTitle(string text) =>
        new()
        {
            Text = text,
            Font = HeroTitleFont,
            ForeColor = UiTheme.TextDark,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Height = 52,
            Margin = new Padding(0, 4, 0, 4)
        };

    public static Label CreateHeroSubtitle(string text) =>
        new()
        {
            Text = text,
            Font = HeroSubtitleFont,
            ForeColor = UiTheme.TextMuted,
            TextAlign = ContentAlignment.MiddleCenter,
            AutoSize = false,
            Height = 30,
            Margin = new Padding(0, 0, 0, 22)
        };
}
