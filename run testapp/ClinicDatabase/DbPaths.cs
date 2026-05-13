using System;
using System.IO;

namespace ClinicVetsAvalonia.Database;

/// <summary>
/// SQLite file location under the current user's application data folder.
/// </summary>
public static class DbPaths
{
    public static string DatabaseFolder =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ClinicVets");

    public static string DatabasePath => Path.Combine(DatabaseFolder, "clinic.db");

    public static string ConnectionString => $"Data Source={DatabasePath}";
}
