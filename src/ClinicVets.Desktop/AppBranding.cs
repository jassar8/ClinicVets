using System.Drawing;

namespace ClinicVets.Desktop;

/// <summary>
/// Loads the embedded multi-size ICO for window chrome (title bar and taskbar).
/// </summary>
internal static class AppBranding
{
    private const string IconResourceName = "ClinicVets.app.ico";
    private static Icon? _template;
    private static Image? _headerImage;

    public static Icon CreateWindowIcon()
    {
        _template ??= LoadTemplate();
        return (Icon)_template.Clone();
    }

    /// <summary>Cached bitmap for in-app header logo (do not dispose from consumers).</summary>
    public static Image GetHeaderImage()
    {
        if (_headerImage is not null)
            return _headerImage;

        using var icon = LoadTemplate();
        _headerImage = icon.ToBitmap();
        return _headerImage;
    }

    private static Icon LoadTemplate()
    {
        var asm = typeof(AppBranding).Assembly;
        using var stream = asm.GetManifestResourceStream(IconResourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource '{IconResourceName}'.");
        return new Icon(stream);
    }
}
