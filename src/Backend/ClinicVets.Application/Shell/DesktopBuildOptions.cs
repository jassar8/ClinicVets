namespace ClinicVets.Application.Shell;

/// <summary>Compile-time switches for desktop hosts. Flip these before shipping a teacher build.</summary>
public static class DesktopBuildOptions
{
    /// <summary>
    /// When <c>true</c>, the login screen shows <b>Enter Demo Mode</b> and in-memory quick access is available.
    /// Set to <c>false</c> for a final teacher build so Demo Mode is not offered in the UI.
    /// </summary>
    public const bool EnableDemoMode = true;

    /// <summary>
    /// When <c>true</c>, self-service registration is saved as <c>Approved</c> with an auto-assigned employee ID
    /// so users can sign in immediately after restart (desktop JSON store).
    /// </summary>
    public const bool AutoApproveSelfRegistration = true;
}
