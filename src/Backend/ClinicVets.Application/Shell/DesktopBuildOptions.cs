namespace ClinicVets.Application.Shell;

/// <summary>Compile-time switches for desktop hosts (WPF / WinForms). Change before a final release build.</summary>
public static class DesktopBuildOptions
{
    /// <summary>
    /// When <c>true</c>, the login screen shows <b>Enter Demo Mode</b> and in-memory quick access is available.
    /// Set to <c>false</c> for a final teacher build so Demo Mode is not offered in the UI.
    /// </summary>
    public const bool EnableDemoMode = true;
}
