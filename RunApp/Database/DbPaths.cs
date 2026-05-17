using System;
using System.IO;

namespace ClinicVetsAvalonia.Database;

/// <summary>
/// SQLite file location under the current user's application data folder.
/// </summary>
public static class DbPaths
{
    private static string? _databaseFolderOverride;

    /// <summary>
    /// For automated tests only: parent folder for <c>clinic.db</c> (isolated from real user data).
    /// Pass <c>null</c> to use the default %AppData%\ClinicVets folder again.
    /// </summary>
    public static void SetDatabaseFolderOverrideForTests(string? absoluteFolder)
    {
        _databaseFolderOverride = string.IsNullOrWhiteSpace(absoluteFolder)
            ? null
            : Path.GetFullPath(absoluteFolder);
    }

    public static string DatabaseFolder =>
        _databaseFolderOverride
        ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClinicVets");

    public static string DatabasePath => Path.Combine(DatabaseFolder, "clinic.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";
}
