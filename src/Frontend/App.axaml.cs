using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using ClinicVets.Desktop.Helpers;
using ClinicVets.Desktop.Stability;

namespace ClinicVets.Desktop;

public partial class App : global::Avalonia.Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        Dispatcher.UIThread.UnhandledException += (_, e) =>
        {
            AppStability.LogException("Dispatcher.UIThread", e.Exception);
            e.Handled = true;
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop &&
                desktop.MainWindow is MainWindow main)
            {
                UIHelper.ShowMessage(main, SafeViewLoader.FriendlyMessage("מערכת"));
            }
        };

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            try
            {
                desktopLifetime.MainWindow = new MainWindow();
            }
            catch (Exception ex)
            {
                AppStability.LogException("MainWindow.ctor", ex, fatal: true);
                throw;
            }
        }

        base.OnFrameworkInitializationCompleted();
    }
}
