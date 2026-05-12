using System.Drawing;

namespace ClinicVets.Desktop;

/// <summary>
/// Loads the embedded multi-size ICO for window chrome (title bar and taskbar).
/// </summary>
internal static class AppBranding
{
    private const string IconResourceName = "ClinicVets.app.ico";
    private static Icon? _template;

    public static Icon CreateWindowIcon()
    {
        _template ??= LoadTemplate();
        return (Icon)_template.Clone();
    }

    private static Icon LoadTemplate()
    {
        var asm = typeof(AppBranding).Assembly;
        using var stream = asm.GetManifestResourceStream(IconResourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource '{IconResourceName}'.");
        return new Icon(stream);
    }
}
