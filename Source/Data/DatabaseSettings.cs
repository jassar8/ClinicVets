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

        private static readonly string LocalAppDataFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClinicVets");

        public static string DatabasePath => ResolveDatabasePath();

        public static string ConnectionString => $"Data Source={DatabasePath}";

        public static bool IsPublishedBuild =>
            File.Exists(Path.Combine(AppContext.BaseDirectory, "ClinicVets.exe"));

        private static string ResolveDatabasePath()
        {
            string? overridePath = Environment.GetEnvironmentVariable("CLINICVETS_DB");
            if (!string.IsNullOrWhiteSpace(overridePath))
                return Path.GetFullPath(overridePath);

            string? portablePath = TryGetPortableDatabasePath();
            if (portablePath != null)
            {
                TryMigrateLegacyDatabase(portablePath);
                return portablePath;
            }

            string localAppDataPath = Path.Combine(LocalAppDataFolder, "ClinicVets.db");
            TryMigrateLegacyDatabase(localAppDataPath);
            return localAppDataPath;
        }

        private static string? TryGetPortableDatabasePath()
        {
            if (!IsPublishedBuild)
                return null;

            return Path.Combine(AppContext.BaseDirectory, "Data", "ClinicVets.db");
        }

        private static void TryMigrateLegacyDatabase(string targetPath)
        {
            if (File.Exists(targetPath))
                return;

            string? targetFolder = Path.GetDirectoryName(targetPath);
            if (string.IsNullOrEmpty(targetFolder))
                return;

            string localAppDataPath = Path.Combine(LocalAppDataFolder, "ClinicVets.db");
            if (File.Exists(localAppDataPath))
            {
                Directory.CreateDirectory(targetFolder);
                File.Copy(localAppDataPath, targetPath);
                return;
            }

            string legacyFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClinicVetsAvalonia");

            string legacyPath = Path.Combine(legacyFolder, "clinic.db");
            if (!File.Exists(legacyPath))
                return;

            Directory.CreateDirectory(targetFolder);
            File.Copy(legacyPath, targetPath);
        }

        public static void EnsureFolderExists()
        {
            string? folder = Path.GetDirectoryName(DatabasePath);
            if (!string.IsNullOrEmpty(folder))
                Directory.CreateDirectory(folder);
        }
    }
}
