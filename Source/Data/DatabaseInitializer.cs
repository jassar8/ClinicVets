using Microsoft.Data.Sqlite;

namespace ClinicVetsAvalonia.Data
{
    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            DatabaseSettings.EnsureFolderExists();
            CreateTables();
            FakeDataSeeder.SeedAllIfEmpty();
        }

        private static void CreateTables()
        {
            using var connection = new SqliteConnection(DatabaseSettings.ConnectionString);
            connection.Open();

            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS Employees (
                    Username TEXT PRIMARY KEY,
                    Password TEXT NOT NULL,
                    EmployeeNumber TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    IdNumber TEXT NOT NULL,
                    Role TEXT NOT NULL
                );");

            // The employee approval system was removed; drop the legacy table if present
            // so existing databases stay consistent with the simplified schema.
            Execute(connection, "DROP TABLE IF EXISTS EmployeeApprovals;");

            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS Clients (
                    IdNumber TEXT PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Gender TEXT NOT NULL DEFAULT 'זכר'
                );");

            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS Animals (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL,
                    Species TEXT NOT NULL,
                    ChipNumber TEXT NOT NULL UNIQUE,
                    Weight REAL NOT NULL,
                    BirthDate TEXT NOT NULL,
                    OwnerIdNumber TEXT NOT NULL,
                    LastVaccinationDate TEXT NOT NULL,
                    FOREIGN KEY (OwnerIdNumber) REFERENCES Clients(IdNumber)
                );");

            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS Medications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    StockQuantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    ExpirationDate TEXT NOT NULL,
                    Notes TEXT NOT NULL
                );");

            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS Visits (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    AnimalChipNumber TEXT NOT NULL,
                    VisitDate TEXT NOT NULL,
                    Reason TEXT NOT NULL,
                    Symptoms TEXT NOT NULL,
                    Diagnosis TEXT NOT NULL,
                    VeterinarianName TEXT NOT NULL,
                    BaseCost REAL NOT NULL,
                    MedicationName TEXT NOT NULL,
                    MedicationQuantity INTEGER NOT NULL,
                    TotalCost REAL NOT NULL,
                    ArrivalStatus TEXT NOT NULL DEFAULT 'Scheduled',
                    ArrivalNote TEXT NOT NULL DEFAULT '',
                    FOREIGN KEY (AnimalChipNumber) REFERENCES Animals(ChipNumber)
                );");

            Execute(connection, @"
                CREATE TABLE IF NOT EXISTS VisitTreatmentLines (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    VisitId INTEGER NOT NULL,
                    Description TEXT NOT NULL,
                    MedicationName TEXT NOT NULL,
                    MedicationQuantity INTEGER NOT NULL,
                    LineCost REAL NOT NULL,
                    FOREIGN KEY (VisitId) REFERENCES Visits(Id) ON DELETE CASCADE
                );");

            SqliteRepositoryBase_EnsureColumn(connection, "Clients", "Gender", "TEXT NOT NULL DEFAULT 'זכר'");
            SqliteRepositoryBase_EnsureColumn(connection, "Visits", "ArrivalStatus", "TEXT NOT NULL DEFAULT 'Scheduled'");
            SqliteRepositoryBase_EnsureColumn(connection, "Visits", "ArrivalNote", "TEXT NOT NULL DEFAULT ''");
        }

        private static void Execute(SqliteConnection connection, string sql)
        {
            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        private static void SqliteRepositoryBase_EnsureColumn(
            SqliteConnection connection,
            string tableName,
            string columnName,
            string columnDefinition)
        {
            bool columnExists = false;
            using var infoCommand = new SqliteCommand($"PRAGMA table_info({tableName});", connection);
            using (var reader = infoCommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (reader.GetString(1) == columnName)
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (!columnExists)
            {
                using var alterCommand = new SqliteCommand(
                    $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};",
                    connection);
                alterCommand.ExecuteNonQuery();
            }
        }
    }
}
