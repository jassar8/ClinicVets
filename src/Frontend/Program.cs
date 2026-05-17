using Avalonia;
using ClinicVets.Desktop.Stability;

namespace ClinicVets.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        AppStability.Initialize();
        AppServices.Initialize();
        BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
    }

    public static AppBuilder BuildAvaloniaApp() =>
        AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
}
