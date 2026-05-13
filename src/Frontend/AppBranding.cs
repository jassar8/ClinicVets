using System.Drawing;

namespace ClinicVets.Desktop;

/// <summary>
/// Loads embedded branding: multi-size ICO for window / EXE chrome and PNG for in-app header clarity.
/// </summary>
internal static class AppBranding
{
    private const string IconResourceName = "ClinicVets.app.ico";
    private const string LogoResourceName = "ClinicVets.logo.png";

    private static Icon? _iconTemplate;
    private static Image? _headerImage;

    public static Icon CreateWindowIcon()
    {
        _iconTemplate ??= LoadIconTemplate();
        return (Icon)_iconTemplate.Clone();
    }

    /// <summary>Cached bitmap for in-app header logo (do not dispose from consumers).</summary>
    public static Image GetHeaderImage()
    {
        if (_headerImage is not null)
            return _headerImage;

        var asm = typeof(AppBranding).Assembly;
        using var stream = asm.GetManifestResourceStream(LogoResourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource '{LogoResourceName}'.");
        using var loaded = Image.FromStream(stream);
        _headerImage = new Bitmap(loaded);
        return _headerImage;
    }

    private static Icon LoadIconTemplate()
    {
        var asm = typeof(AppBranding).Assembly;
        using var stream = asm.GetManifestResourceStream(IconResourceName)
            ?? throw new InvalidOperationException($"Missing embedded resource '{IconResourceName}'.");
        return new Icon(stream);
    }
}
