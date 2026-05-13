namespace ClinicVets.Desktop.UI;

/// <summary>Rounded text field chrome (v2 naming; wraps <see cref="RoundedInputHost"/>).</summary>
public sealed class ModernTextField : RoundedInputHost
{
    public ModernTextField(TextBox inner, bool showPasswordRevealToggle = false)
        : base(inner, showPasswordRevealToggle)
    {
    }
}
