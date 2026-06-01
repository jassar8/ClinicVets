using System;
using System.IO;

namespace ClinicVetsAvalonia.Data
{
    public static class DatabaseSettings
    {
        /// <summary>
        /// When true and the database has no employees, demo accounts are inserted once.
        /// </summary>
        public static bool SeedDemoDataWhenEmpty { get; set; } = true;

        /// <summary>
        /// When true, ensures the five fake demo users exist (adds any that are missing).
        /// </summary>
        public static bool SeedFakeUsersOnStartup { get; set; } = true;

        private static readonly string DatabaseFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClinicVets");

        public static string DatabasePath => ResolveDatabasePath();

        public static string ConnectionString => $"Data Source={DatabasePath}";

        private static string ResolveDatabasePath()
        {
            string? overridePath = Environment.GetEnvironmentVariable("CLINICVETS_DB");
            if (!string.IsNullOrWhiteSpace(overridePath))
                return Path.GetFullPath(overridePath);

            string newPath = Path.Combine(DatabaseFolder, "ClinicVets.db");
            TryMigrateLegacyDatabase(newPath);
            return newPath;
        }

        private static void TryMigrateLegacyDatabase(string newPath)
        {
            if (File.Exists(newPath))
                return;

            string legacyFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClinicVetsAvalonia");

            string legacyPath = Path.Combine(legacyFolder, "clinic.db");
            if (!File.Exists(legacyPath))
                return;

            Directory.CreateDirectory(Path.GetDirectoryName(newPath)!);
            File.Copy(legacyPath, newPath);
        }

        public static void EnsureFolderExists()
        {
            Directory.CreateDirectory(DatabaseFolder);
        }
    }
}
