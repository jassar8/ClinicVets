using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using ClinicManagementSystem.app.Models;

namespace ClinicManagementSystem.app.Data
{
    public static class AppData
    {
        public static List<Employee> Employees { get; set; } = new List<Employee>();
        public static List<Client> Clients { get; set; } = new List<Client>();

        private static readonly string DatabaseFolder =
      Path.Combine(
          Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
          "ClinicManagementSystem"
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

            using var employeesCommand = new SqliteCommand(createEmployeesTable, connection);
            employeesCommand.ExecuteNonQuery();

            using var clientsCommand = new SqliteCommand(createClientsTable, connection);
            clientsCommand.ExecuteNonQuery();
        }

        private static void AddDefaultEmployees()
        {
            Employees.Add(new Employee
            {
                Username = "admin",
                Password = "1234",
                EmployeeNumber = "0001",
                Email = "admin@clinic.com",
                Role = "Secretary"
            });

            Employees.Add(new Employee
            {
                Username = "vet",
                Password = "1234",
                EmployeeNumber = "0002",
                Email = "vet@clinic.com",
                Role = "Vet"
            });
        }

        public static void LoadEmployees()
        {
            Employees.Clear();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "SELECT Username, Password, EmployeeNumber, Email, Role FROM Employees";

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
                    Role = reader.GetString(4)
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
                    (Username, Password, EmployeeNumber, Email, Role)
                    VALUES
                    (@Username, @Password, @EmployeeNumber, @Email, @Role);
                ";

                using var command = new SqliteCommand(insertQuery, connection);

                command.Parameters.AddWithValue("@Username", employee.Username);
                command.Parameters.AddWithValue("@Password", employee.Password);
                command.Parameters.AddWithValue("@EmployeeNumber", employee.EmployeeNumber);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@Role", employee.Role);

                command.ExecuteNonQuery();
            }
        }

        public static void LoadClients()
        {
            Clients.Clear();

            using var connection = new SqliteConnection(ConnectionString);
            connection.Open();

            string query = "SELECT FullName, IdNumber, Phone, Email FROM Clients";

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
    }
}