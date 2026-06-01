using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data.Repositories
{
    internal sealed class EmployeeRepository : SqliteRepositoryBase
    {
        public List<Employee> LoadAll()
        {
            var employees = new List<Employee>();

            using var connection = OpenConnection();

            string query = @"
                SELECT e.Username, e.Password, e.EmployeeNumber, e.Email, e.IdNumber, e.Role,
                       COALESCE(a.IsApproved, 1)
                FROM Employees e
                LEFT JOIN EmployeeApprovals a ON a.Username = e.Username;";

            using var command = new SqliteCommand(query, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                employees.Add(new Employee
                {
                    Username = reader.GetString(0),
                    Password = reader.GetString(1),
                    EmployeeNumber = reader.GetString(2),
                    Email = reader.GetString(3),
                    IdNumber = reader.GetString(4),
                    Role = reader.GetString(5),
                    IsApproved = reader.GetInt32(6) == 1
                });
            }

            return employees;
        }

        public void SaveAll(IReadOnlyList<Employee> employees)
        {
            using var connection = OpenConnection();

            using var deleteApprovals = new SqliteCommand("DELETE FROM EmployeeApprovals", connection);
            deleteApprovals.ExecuteNonQuery();

            using var deleteEmployees = new SqliteCommand("DELETE FROM Employees", connection);
            deleteEmployees.ExecuteNonQuery();

            foreach (var employee in employees)
                Insert(connection, employee);
        }

        public void Insert(Employee employee)
        {
            using var connection = OpenConnection();
            Insert(connection, employee);
        }

        internal void Insert(SqliteConnection connection, Employee employee)
        {
            string insertEmployee = @"
                INSERT INTO Employees
                (Username, Password, EmployeeNumber, Email, IdNumber, Role)
                VALUES
                (@Username, @Password, @EmployeeNumber, @Email, @IdNumber, @Role);";

            using (var command = new SqliteCommand(insertEmployee, connection))
            {
                command.Parameters.AddWithValue("@Username", employee.Username);
                command.Parameters.AddWithValue("@Password", employee.Password);
                command.Parameters.AddWithValue("@EmployeeNumber", employee.EmployeeNumber);
                command.Parameters.AddWithValue("@Email", employee.Email);
                command.Parameters.AddWithValue("@IdNumber", employee.IdNumber);
                command.Parameters.AddWithValue("@Role", employee.Role);
                command.ExecuteNonQuery();
            }

            UpsertApproval(connection, employee);
        }

        private static void UpsertApproval(SqliteConnection connection, Employee employee)
        {
            string insertApproval = @"
                INSERT INTO EmployeeApprovals (Username, IsApproved, ApprovedBy, ApprovedAt)
                VALUES (@Username, @IsApproved, @ApprovedBy, @ApprovedAt)
                ON CONFLICT(Username) DO UPDATE SET
                    IsApproved = excluded.IsApproved,
                    ApprovedBy = excluded.ApprovedBy,
                    ApprovedAt = excluded.ApprovedAt;";

            using var command = new SqliteCommand(insertApproval, connection);
            command.Parameters.AddWithValue("@Username", employee.Username);
            command.Parameters.AddWithValue("@IsApproved", employee.IsApproved ? 1 : 0);
            command.Parameters.AddWithValue("@ApprovedBy", employee.IsApproved ? "system" : "");
            command.Parameters.AddWithValue(
                "@ApprovedAt",
                employee.IsApproved ? DateTime.UtcNow.ToString("o") : (object)DBNull.Value);
            command.ExecuteNonQuery();
        }
    }
}
