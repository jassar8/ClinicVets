using Avalonia;
using Avalonia.Rendering.Composition;
using Avalonia.Win32;
using System;

namespace ClinicVetsAvalonia;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
#if DEBUG
            .WithDeveloperTools()
#endif
            .With(new CompositionOptions
            {
                UseSaveLayerRootClip = false
            })
            .With(new Win32PlatformOptions
            {
                DpiAwareness = Win32DpiAwareness.PerMonitorDpiAware
            })
            .WithInterFont()
            .LogToTrace();
}
