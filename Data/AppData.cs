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
                    Email TEXT NOT NULL
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
                    FOREIGN KEY (AnimalChipNumber) REFERENCES Animals(ChipNumber)
                );
            ";

            using var employeesCommand = new SqliteCommand(createEmployeesTable, connection);
            employeesCommand.ExecuteNonQuery();

            using var clientsCommand = new SqliteCommand(createClientsTable, connection);
            clientsCommand.ExecuteNonQuery();

            using var animalsCommand = new SqliteCommand(createAnimalsTable, connection);
            animalsCommand.ExecuteNonQuery();

            using var medicationsCommand = new SqliteCommand(createMedicationsTable, connection);
            medicationsCommand.ExecuteNonQuery();

            using var visitsCommand = new SqliteCommand(createVisitsTable, connection);
            visitsCommand.ExecuteNonQuery();
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
                SELECT FullName, IdNumber, Phone, Email
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
                    Email = reader.GetString(3)
                });
            }
        }

        public static void SaveClientsToDatabase()
        {
            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            using var deleteCommand = new SqliteCommand("DELETE FROM Clients", connection);
            deleteCommand.ExecuteNonQuery();

            foreach (var client in Clients)
            {
                string insertQuery = @"
                    INSERT INTO Clients
                    (IdNumber, FullName, Phone, Email)
                    VALUES
                    (@IdNumber, @FullName, @Phone, @Email);
                ";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@IdNumber", client.IdNumber);
                command.Parameters.AddWithValue("@FullName", client.FullName);
                command.Parameters.AddWithValue("@Phone", client.Phone);
                command.Parameters.AddWithValue("@Email", client.Email);

                command.ExecuteNonQuery();
            }
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

            using var deleteCommand = new SqliteCommand("DELETE FROM Animals", connection);
            deleteCommand.ExecuteNonQuery();

            foreach (var animal in Animals)
            {
                string insertQuery = @"
                    INSERT INTO Animals
                    (Name, Species, ChipNumber, Weight, BirthDate, OwnerIdNumber, LastVaccinationDate)
                    VALUES
                    (@Name, @Species, @ChipNumber, @Weight, @BirthDate, @OwnerIdNumber, @LastVaccinationDate);
                ";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@Name", animal.Name);
                command.Parameters.AddWithValue("@Species", animal.Species);
                command.Parameters.AddWithValue("@ChipNumber", animal.ChipNumber);
                command.Parameters.AddWithValue("@Weight", animal.Weight);
                command.Parameters.AddWithValue("@BirthDate", animal.BirthDate.ToString("yyyy-MM-dd"));
                command.Parameters.AddWithValue("@OwnerIdNumber", animal.OwnerIdNumber);
                command.Parameters.AddWithValue("@LastVaccinationDate", animal.LastVaccinationDate.ToString("yyyy-MM-dd"));

                command.ExecuteNonQuery();
            }
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
                       VeterinarianName, BaseCost, MedicationName, MedicationQuantity, TotalCost
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
                    TotalCost = reader.GetDouble(10)
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
                     BaseCost, MedicationName, MedicationQuantity, TotalCost)
                    VALUES
                    (@Id, @AnimalChipNumber, @VisitDate, @Reason, @Symptoms, @Diagnosis, @VeterinarianName,
                     @BaseCost, @MedicationName, @MedicationQuantity, @TotalCost);
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

                command.ExecuteNonQuery();
            }
        }
    }
}