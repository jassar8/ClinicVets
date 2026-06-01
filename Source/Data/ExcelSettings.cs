using System;
using System.IO;

namespace ClinicVetsAvalonia.Data
{
    public static class ExcelSettings
    {
        public const string FileName = "ClinicVets.xlsx";

        public static string ProjectExcelPath => ResolveProjectExcelPath();

        public static string RuntimeExcelPath => Path.Combine(
            Path.GetDirectoryName(DatabaseSettings.DatabasePath)!,
            FileName);

        public static string ActiveExcelPath => ResolveActivePath();

        private static string ResolveActivePath()
        {
            if (DatabaseSettings.IsPublishedBuild)
            {
                string portableExcel = Path.Combine(AppContext.BaseDirectory, "Data", FileName);
                Directory.CreateDirectory(Path.GetDirectoryName(portableExcel)!);
                return portableExcel;
            }

            if (TryEnsureWritable(ProjectExcelPath))
                return ProjectExcelPath;

            Directory.CreateDirectory(Path.GetDirectoryName(RuntimeExcelPath)!);
            return RuntimeExcelPath;
        }

        private static string ResolveProjectExcelPath()
        {
            string? solutionDir = FindSolutionDirectory(AppContext.BaseDirectory);
            if (solutionDir == null)
                return RuntimeExcelPath;

            return Path.Combine(solutionDir, "Source", "Data", FileName);
        }

        private static string? FindSolutionDirectory(string startDirectory)
        {
            var directory = new DirectoryInfo(startDirectory);
            while (directory != null)
            {
                if (File.Exists(Path.Combine(directory.FullName, "ClinicVets.sln")))
                    return directory.FullName;

                directory = directory.Parent;
            }

            return null;
        }

        private static bool TryEnsureWritable(string filePath)
        {
            try
            {
                string? folder = Path.GetDirectoryName(filePath);
                if (string.IsNullOrEmpty(folder))
                    return false;

                Directory.CreateDirectory(folder);

                if (!File.Exists(filePath))
                {
                    File.WriteAllBytes(filePath, Array.Empty<byte>());
                    File.Delete(filePath);
                }

                using var stream = new FileStream(
                    filePath,
                    FileMode.OpenOrCreate,
                    FileAccess.ReadWrite,
                    FileShare.None);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
