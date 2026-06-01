using System.Collections.Generic;
using System.Linq;
using ClinicVetsAvalonia.Data.Repositories;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Data
{
    public static class FakeDataSeeder
    {
        /// <summary>Shared password for all seeded demo users (meets registration rules).</summary>
        public const string DefaultPassword = "Pass123!";

        public static IReadOnlyList<Employee> CreateFiveEmployees() =>
        [
            new Employee
            {
                Username = "sarah1",
                Password = DefaultPassword,
                EmployeeNumber = "1001",
                Email = "sarah@clinic.com",
                IdNumber = "301234567",
                Role = "Secretary",
                IsApproved = true
            },
            new Employee
            {
                Username = "david2",
                Password = DefaultPassword,
                EmployeeNumber = "1002",
                Email = "david@clinic.com",
                IdNumber = "302345678",
                Role = "Vet",
                IsApproved = true
            },
            new Employee
            {
                Username = "noa345",
                Password = DefaultPassword,
                EmployeeNumber = "1003",
                Email = "noa@clinic.com",
                IdNumber = "303456789",
                Role = "Secretary",
                IsApproved = true
            },
            new Employee
            {
                Username = "mike99",
                Password = DefaultPassword,
                EmployeeNumber = "1004",
                Email = "mike@clinic.com",
                IdNumber = "304567890",
                Role = "Vet",
                IsApproved = true
            },
            new Employee
            {
                Username = "lior12",
                Password = DefaultPassword,
                EmployeeNumber = "1005",
                Email = "lior@clinic.com",
                IdNumber = "305678901",
                Role = "Vet",
                IsApproved = true
            }
        ];

        internal static int SeedMissingEmployees(EmployeeRepository repository)
        {
            var existingUsernames = repository.LoadAll()
                .Select(employee => employee.Username)
                .ToHashSet();

            int added = 0;
            foreach (var employee in CreateFiveEmployees())
            {
                if (existingUsernames.Contains(employee.Username))
                    continue;

                repository.Insert(employee);
                existingUsernames.Add(employee.Username);
                added++;
            }

            return added;
        }
    }
}
