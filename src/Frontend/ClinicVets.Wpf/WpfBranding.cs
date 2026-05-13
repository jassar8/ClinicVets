using System.IO;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace ClinicVets.Wpf;

internal static class WpfBranding
{
    private const string LogoResourceName = "ClinicVets.logo.png";
    private const string IconResourceName = "ClinicVets.app.ico";

    public static BitmapImage? LoadLogo()
    {
        var asm = Assembly.GetExecutingAssembly();
        using var stream = asm.GetManifestResourceStream(LogoResourceName);
        if (stream is null)
            return null;
        using var ms = new MemoryStream();
        stream.CopyTo(ms);
        ms.Position = 0;
        var img = new BitmapImage();
        img.BeginInit();
        img.StreamSource = ms;
        img.CacheOption = BitmapCacheOption.OnLoad;
        img.EndInit();
        img.Freeze();
        return img;
    }

    public static Stream? OpenIconStream() =>
        Assembly.GetExecutingAssembly().GetManifestResourceStream(IconResourceName);
}
