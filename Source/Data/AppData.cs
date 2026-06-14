using System.Collections.Generic;
using ClinicVetsAvalonia.Data.Repositories;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data
{
    // In-memory cache of all app data and the single entry point that coordinates
    // the SQLite repositories and the Excel export. Screens read/write these lists.
    public static class AppData
    {
        private static readonly EmployeeRepository EmployeeRepository = new();
        private static readonly ClientRepository ClientRepository = new();
        private static readonly AnimalRepository AnimalRepository = new();
        private static readonly MedicationRepository MedicationRepository = new();
        private static readonly VisitRepository VisitRepository = new();

        public static List<Employee> Employees { get; set; } = new();
        public static List<Client> Clients { get; set; } = new();
        public static List<Animal> Animals { get; set; } = new();
        public static List<Medication> Medications { get; set; } = new();
        public static List<Visit> Visits { get; set; } = new();

        public static string DatabaseFilePath => DatabaseSettings.DatabasePath;

        public static string ExcelFilePath => ExcelSettings.ActiveExcelPath;

        // App startup: make sure the database exists and is seeded, load everything into
        // memory, then mirror it to Excel. Called once when the main window opens.
        public static void Initialize()
        {
            DatabaseInitializer.Initialize();
            ReloadAll();
            ExcelExportService.ExportAll();
        }

        // Reloads every list from the database (used after startup and re-seeding).
        public static void ReloadAll()
        {
            LoadEmployees();
            LoadClients();
            LoadAnimals();
            LoadMedications();
            LoadVisits();
        }

        public static void LoadEmployees()
        {
            Employees = EmployeeRepository.LoadAll();
        }

        // Shared save pattern used by every entity: persist to SQLite, reload to get the
        // canonical state back, then re-export the Excel mirror so the two stay consistent.
        public static void SaveEmployeesToDatabase()
        {
            EmployeeRepository.SaveAll(Employees);
            LoadEmployees();
            ExcelExportService.ExportAll();
        }

        public static void LoadClients()
        {
            Clients = ClientRepository.LoadAll();
        }

        public static void SaveClientsToDatabase()
        {
            ClientRepository.SaveAll(Clients);
            LoadClients();
            ExcelExportService.ExportAll();
        }

        public static void LoadAnimals()
        {
            Animals = AnimalRepository.LoadAll();
        }

        public static void SaveAnimalsToDatabase()
        {
            AnimalRepository.SaveAll(Animals);
            LoadAnimals();
            ExcelExportService.ExportAll();
        }

        public static void LoadMedications()
        {
            Medications = MedicationRepository.LoadAll();
        }

        public static void SaveMedicationsToDatabase()
        {
            MedicationRepository.SaveAll(Medications);
            LoadMedications();
            ExcelExportService.ExportAll();
        }

        public static void LoadVisits()
        {
            Visits = VisitRepository.LoadAll(Medications);
        }

        public static void SaveVisitsToDatabase()
        {
            VisitRepository.SaveAll(Visits);
            LoadVisits();
            ExcelExportService.ExportAll();
        }
    }
}
