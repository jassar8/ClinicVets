using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data
{
    public static class AppData
    {
        public static List<Employee> Employees { get; set; } = new List<Employee>();
        public static List<Client> Clients { get; set; } = new List<Client>();
        public static List<Animal> Animals { get; set; } = new List<Animal>();
        public static List<Medication> Medications { get; set; } = new List<Medication>();
        public static List<Visit> Visits { get; set; } = new List<Visit>();

        private static readonly string DatabaseFolder =
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ClinicVetsAvalonia"
            );

        private static readonly string DatabasePath =
            Path.Combine(DatabaseFolder, "clinic.db");

        private static readonly string ConnectionString =
            $"Data Source={DatabasePath}";

        public static void Initialize()
        {
            Directory.CreateDirectory(DatabaseFolder);

            CreateTables();

            LoadEmployees();
            LoadClients();
            LoadAnimals();
            LoadMedications();
            LoadVisits();

            if (Employees.Count == 0)
            {
                AddDefaultEmployees();
                SaveEmployeesToDatabase();
                LoadEmployees();
            }
        }

        private static void CreateTables()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string createEmployeesTable = @"
                CREATE TABLE IF NOT EXISTS Employees (
                    Username TEXT PRIMARY KEY,
                    Password TEXT NOT NULL,
                    EmployeeNumber TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    IdNumber TEXT NOT NULL,
                    Role TEXT NOT NULL
                );
            ";

            string createClientsTable = @"
                CREATE TABLE IF NOT EXISTS Clients (
                    IdNumber TEXT PRIMARY KEY,
                    FullName TEXT NOT NULL,
                    Phone TEXT NOT NULL,
                    Email TEXT NOT NULL,
                    Gender TEXT NOT NULL DEFAULT 'זכר'
                );
            ";

            string createAnimalsTable = @"
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

            string createMedicationsTable = @"
                CREATE TABLE IF NOT EXISTS Medications (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Name TEXT NOT NULL UNIQUE,
                    StockQuantity INTEGER NOT NULL,
                    UnitPrice REAL NOT NULL,
                    ExpirationDate TEXT NOT NULL,
                    Notes TEXT NOT NULL
                );
            ";

            string createVisitsTable = @"
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
                );
            ";

            using var employeesCommand = new SqliteCommand(createEmployeesTable, connection);
            employeesCommand.ExecuteNonQuery();

            using var clientsCommand = new SqliteCommand(createClientsTable, connection);
            clientsCommand.ExecuteNonQuery();
            EnsureColumnExists(connection, "Clients", "Gender", "TEXT NOT NULL DEFAULT 'זכר'");

            using var animalsCommand = new SqliteCommand(createAnimalsTable, connection);
            animalsCommand.ExecuteNonQuery();

            using var medicationsCommand = new SqliteCommand(createMedicationsTable, connection);
            medicationsCommand.ExecuteNonQuery();

            using var visitsCommand = new SqliteCommand(createVisitsTable, connection);
            visitsCommand.ExecuteNonQuery();
            EnsureColumnExists(connection, "Visits", "ArrivalStatus", "TEXT NOT NULL DEFAULT 'Scheduled'");
            EnsureColumnExists(connection, "Visits", "ArrivalNote", "TEXT NOT NULL DEFAULT ''");
        }

        private static void EnsureColumnExists(SqliteConnection connection, string tableName, string columnName, string columnDefinition)
        {
            bool columnExists = false;

            using var infoCommand = new SqliteCommand($"PRAGMA table_info({tableName});", connection);
            {
                using var reader = infoCommand.ExecuteReader();

                while (reader.Read())
                {
                    if (reader.GetString(1) == columnName)
                    {
                        columnExists = true;
                        break;
                    }
                }
            }

            if (columnExists)
                return;

            using var alterCommand = new SqliteCommand(
                $"ALTER TABLE {tableName} ADD COLUMN {columnName} {columnDefinition};",
                connection);
            alterCommand.ExecuteNonQuery();
        }

        private static void AddDefaultEmployees()
        {
            Employees.Add(new Employee
            {
                Username = "admin",
                Password = "1234",
                EmployeeNumber = "0001",
                Email = "admin@clinic.com",
                IdNumber = "000000001",
                Role = "Secretary"
            });

            Employees.Add(new Employee
            {
                Username = "vet",
                Password = "1234",
                EmployeeNumber = "0002",
                Email = "vet@clinic.com",
                IdNumber = "000000002",
                Role = "Vet"
            });
        }

        public static void LoadEmployees()
        {
            Employees.Clear();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT Username, Password, EmployeeNumber, Email, IdNumber, Role
                FROM Employees;
            ";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Employees.Add(new Employee
                {
                    Username = reader.GetString(0),
                    Password = reader.GetString(1),
                    EmployeeNumber = reader.GetString(2),
                    Email = reader.GetString(3),
                    IdNumber = reader.GetString(4),
                    Role = reader.GetString(5)
                });
            }
        }

        public static void SaveEmployeesToDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var deleteCommand = new SqliteCommand("DELETE FROM Employees", connection);
            deleteCommand.ExecuteNonQuery();

            foreach (var employee in Employees)
            {
                string insertQuery = @"
                    INSERT INTO Employees
                    (Username, Password, EmployeeNumber, Email, IdNumber, Role)
                    VALUES
                    (@Username, @Password, @EmployeeNumber, @Email, @IdNumber, @Role);
                ";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@Username", employee.Username);
                command.Parameters.AddWithValue("@Password", employee.Password);
                command.Parameters.AddWithValue("@EmployeeNumber", employee.EmployeeNumber);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@IdNumber", employee.IdNumber);
                command.Parameters.AddWithValue("@Role", employee.Role);

                command.ExecuteNonQuery();
            }
        }

        public static void LoadClients()
        {
            Clients.Clear();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT FullName, IdNumber, Phone, Email, Gender
                FROM Clients;
            ";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Clients.Add(new Client
                {
                    FullName = reader.GetString(0),
                    IdNumber = reader.GetString(1),
                    Phone = reader.GetString(2),
                    Email = reader.GetString(3),
                    Gender = reader.GetString(4)
                });
            }
        }

        public static void SaveClientsToDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            foreach (var client in Clients)
            {
                string insertQuery = @"
                    INSERT INTO Clients
                    (IdNumber, FullName, Phone, Email, Gender)
                    VALUES
                    (@IdNumber, @FullName, @Phone, @Email, @Gender)
                    ON CONFLICT(IdNumber) DO UPDATE SET
                        FullName = excluded.FullName,
                        Phone = excluded.Phone,
                        Email = excluded.Email,
                        Gender = excluded.Gender;
                ";

                using var command = new SqliteCommand(insertQuery, connection);
                command.Transaction = transaction;

                command.Parameters.AddWithValue("@IdNumber", client.IdNumber);
                command.Parameters.AddWithValue("@FullName", client.FullName);
                command.Parameters.AddWithValue("@Phone", client.Phone);
                command.Parameters.AddWithValue("@Email", client.Email);
                command.Parameters.AddWithValue("@Gender", client.Gender);

                command.ExecuteNonQuery();
            }

            var savedIdNumbers = new HashSet<string>();
            foreach (var client in Clients)
                savedIdNumbers.Add(client.IdNumber);

            var databaseIdNumbers = new List<string>();
            using (var selectCommand = new SqliteCommand("SELECT IdNumber FROM Clients;", connection))
            {
                selectCommand.Transaction = transaction;
                using var reader = selectCommand.ExecuteReader();

                while (reader.Read())
                    databaseIdNumbers.Add(reader.GetString(0));
            }

            foreach (string idNumber in databaseIdNumbers)
            {
                if (savedIdNumbers.Contains(idNumber))
                    continue;

                using var deleteCommand = new SqliteCommand(
                    "DELETE FROM Clients WHERE IdNumber = @IdNumber;",
                    connection);
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@IdNumber", idNumber);
                deleteCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static void LoadAnimals()
        {
            Animals.Clear();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT Id, Name, Species, ChipNumber, Weight, BirthDate, OwnerIdNumber, LastVaccinationDate
                FROM Animals;
            ";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Animals.Add(new Animal
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    Species = reader.GetString(2),
                    ChipNumber = reader.GetString(3),
                    Weight = reader.GetDouble(4),
                    BirthDate = DateTime.Parse(reader.GetString(5)),
                    OwnerIdNumber = reader.GetString(6),
                    LastVaccinationDate = DateTime.Parse(reader.GetString(7))
                });
            }
        }

        public static void SaveAnimalsToDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var transaction = connection.BeginTransaction();

            foreach (var animal in Animals)
            {
                string insertQuery = @"
                    INSERT INTO Animals
                    (Name, Species, ChipNumber, Weight, BirthDate, OwnerIdNumber, LastVaccinationDate)
                    VALUES
                    (@Name, @Species, @ChipNumber, @Weight, @BirthDate, @OwnerIdNumber, @LastVaccinationDate)
                    ON CONFLICT(ChipNumber) DO UPDATE SET
                        Name = excluded.Name,
                        Species = excluded.Species,
                        Weight = excluded.Weight,
                        BirthDate = excluded.BirthDate,
                        OwnerIdNumber = excluded.OwnerIdNumber,
                        LastVaccinationDate = excluded.LastVaccinationDate;
                ";

                using var command = new SqliteCommand(insertQuery, connection);
                command.Transaction = transaction;

                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Species", animal.Species);
                command.Parameters.AddWithValue("@ChipNumber", animal.ChipNumber);
                command.Parameters.AddWithValue("@Weight", animal.Weight);
                command.Parameters.AddWithValue("@BirthDate", animal.BirthDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@OwnerIdNumber", animal.OwnerIdNumber);
                command.Parameters.AddWithValue("@LastVaccinationDate", animal.LastVaccinationDate.ToString("yyyy-MM-dd"));

                command.ExecuteNonQuery();
            }

            var savedChipNumbers = new HashSet<string>();
            foreach (var animal in Animals)
                savedChipNumbers.Add(animal.ChipNumber);

            var databaseChipNumbers = new List<string>();
            using (var selectCommand = new SqliteCommand("SELECT ChipNumber FROM Animals;", connection))
            {
                selectCommand.Transaction = transaction;
                using var reader = selectCommand.ExecuteReader();

                while (reader.Read())
                    databaseChipNumbers.Add(reader.GetString(0));
            }

            foreach (string chipNumber in databaseChipNumbers)
            {
                if (savedChipNumbers.Contains(chipNumber))
                    continue;

                using var deleteCommand = new SqliteCommand(
                    "DELETE FROM Animals WHERE ChipNumber = @ChipNumber;",
                    connection);
                deleteCommand.Transaction = transaction;
                deleteCommand.Parameters.AddWithValue("@ChipNumber", chipNumber);
                deleteCommand.ExecuteNonQuery();
            }

            transaction.Commit();
        }

        public static void LoadMedications()
        {
            Medications.Clear();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT Id, Name, StockQuantity, UnitPrice, ExpirationDate, Notes
                FROM Medications;
            ";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Medications.Add(new Medication
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1),
                    StockQuantity = reader.GetInt32(2),
                    UnitPrice = reader.GetDouble(3),
                    ExpirationDate = DateTime.Parse(reader.GetString(4)),
                    Notes = reader.GetString(5)
                });
            }
        }

        public static void SaveMedicationsToDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var deleteCommand = new SqliteCommand("DELETE FROM Medications", connection);
            deleteCommand.ExecuteNonQuery();

            foreach (var medication in Medications)
            {
                string insertQuery = @"
                    INSERT INTO Medications
                    (Id, Name, StockQuantity, UnitPrice, ExpirationDate, Notes)
                    VALUES
                    (@Id, @Name, @StockQuantity, @UnitPrice, @ExpirationDate, @Notes);
                ";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@Id", medication.Id == 0 ? (object)DBNull.Value : medication.Id);
                command.Parameters.AddWithValue("@Name", medication.Name);
                command.Parameters.AddWithValue("@StockQuantity", medication.StockQuantity);
                command.Parameters.AddWithValue("@UnitPrice", medication.UnitPrice);
                command.Parameters.AddWithValue("@ExpirationDate", medication.ExpirationDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@Notes", medication.Notes);

                command.ExecuteNonQuery();
            }
        }

        public static void LoadVisits()
        {
            Visits.Clear();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = @"
                SELECT Id, AnimalChipNumber, VisitDate, Reason, Symptoms, Diagnosis,
                       VeterinarianName, BaseCost, MedicationName, MedicationQuantity, TotalCost,
                       ArrivalStatus, ArrivalNote
                FROM Visits;
            ";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                Visits.Add(new Visit
                {
                    Id = reader.GetInt32(0),
                    AnimalChipNumber = reader.GetString(1),
                    VisitDate = DateTime.Parse(reader.GetString(2)),
                    Reason = reader.GetString(3),
                    Symptoms = reader.GetString(4),
                    Diagnosis = reader.GetString(5),
                    VeterinarianName = reader.GetString(6),
                    BaseCost = reader.GetDouble(7),
                    MedicationName = reader.GetString(8),
                    MedicationQuantity = reader.GetInt32(9),
                    TotalCost = reader.GetDouble(10),
                    ArrivalStatus = reader.GetString(11),
                    ArrivalNote = reader.GetString(12)
                });
            }
        }

        public static void SaveVisitsToDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var deleteCommand = new SqliteCommand("DELETE FROM Visits", connection);
            deleteCommand.ExecuteNonQuery();

            foreach (var visit in Visits)
            {
                string insertQuery = @"
                    INSERT INTO Visits
                    (Id, AnimalChipNumber, VisitDate, Reason, Symptoms, Diagnosis, VeterinarianName,
                     BaseCost, MedicationName, MedicationQuantity, TotalCost, ArrivalStatus, ArrivalNote)
                    VALUES
                    (@Id, @AnimalChipNumber, @VisitDate, @Reason, @Symptoms, @Diagnosis, @VeterinarianName,
                     @BaseCost, @MedicationName, @MedicationQuantity, @TotalCost, @ArrivalStatus, @ArrivalNote);
                ";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@Id", visit.Id == 0 ? (object)DBNull.Value : visit.Id);
                command.Parameters.AddWithValue("@AnimalChipNumber", visit.AnimalChipNumber);
                command.Parameters.AddWithValue("@VisitDate", visit.VisitDate.ToString("yyyy-MM-dd HH:mm"));
                command.Parameters.AddWithValue("@Reason", visit.Reason);
                command.Parameters.AddWithValue("@Symptoms", visit.Symptoms);
                command.Parameters.AddWithValue("@Diagnosis", visit.Diagnosis);
                command.Parameters.AddWithValue("@VeterinarianName", visit.VeterinarianName);
                command.Parameters.AddWithValue("@BaseCost", visit.BaseCost);
                command.Parameters.AddWithValue("@MedicationName", visit.MedicationName);
                command.Parameters.AddWithValue("@MedicationQuantity", visit.MedicationQuantity);
                command.Parameters.AddWithValue("@TotalCost", visit.TotalCost);
                command.Parameters.AddWithValue("@ArrivalStatus", visit.ArrivalStatus);
                command.Parameters.AddWithValue("@ArrivalNote", visit.ArrivalNote);

                command.ExecuteNonQuery();
            }
        }
    }
}