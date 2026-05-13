namespace ClinicVets.Desktop;

/// <summary>Tracks quick-access demo workspace (in-memory data, not a real authenticated session).</summary>
public static class DemoModeSession
{
    public static bool IsActive { get; private set; }

    public static void Enter() => IsActive = true;

    public static void Exit() => IsActive = false;
}
