using Microsoft.Data.Sqlite;

namespace ClinicVetsAvalonia.Database;

/// <summary>
/// Creates clinic tables if they do not exist.
/// </summary>
public static class ClinicSchema
{
    public static void EnsureTables(SqliteConnection connection)
    {
        const string createEmployeesTable = @"
                CREATE TABLE IF NOT EXISTS Employees (
                    Username TEXT PRIMARY KEY,
                    Password TEXT NOT NULL,
                    EmployeeNumber TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    IdNumber TEXT NOT NULL,
                    Role TEXT NOT NULL
                );
            ";

        const string createClientsTable = @"
                CREATE TABLE IF NOT EXISTS Clients (
                    IdNumber TEXT PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    Email TEXT NOT NULL
                );
            ";

        const string createAnimalsTable = @"
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
                );
            ";

        const string createMedicationsTable = @"
                CREATE TABLE IF NOT EXISTS Medications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    StockQuantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    ExpirationDate TEXT NOT NULL,
                    Notes TEXT NOT NULL
                );
            ";

        const string createVisitsTable = @"
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
                    FOREIGN KEY (AnimalChipNumber) REFERENCES Animals(ChipNumber)
                );
            ";

        using (var cmd = new SqliteCommand(createEmployeesTable, connection))
            cmd.ExecuteNonQuery();

        using (var cmd = new SqliteCommand(createClientsTable, connection))
            cmd.ExecuteNonQuery();

        using (var cmd = new SqliteCommand(createAnimalsTable, connection))
            cmd.ExecuteNonQuery();

        using (var cmd = new SqliteCommand(createMedicationsTable, connection))
            cmd.ExecuteNonQuery();

        using (var cmd = new SqliteCommand(createVisitsTable, connection))
            cmd.ExecuteNonQuery();
    }
}
