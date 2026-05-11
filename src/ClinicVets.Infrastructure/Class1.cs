using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

public class InMemoryEmployeeRepository : IEmployeeRepository
{
    // Demo users for quick presentation and software testing exercises.
    private readonly List<Employee> _employees =
    [
        new Employee
        {
            FullName = "Dr. Amir Levi",
            Email = "vet@clinicvets.com",
            Password = "Vet123!",
            Role = "Veterinarian"
        },
        new Employee
        {
            FullName = "Maya Cohen",
            Email = "secretary@clinicvets.com",
            Password = "Sec123!",
            Role = "Secretary"
        }
    ];

    public Task<Employee?> GetByEmailAsync(string email)
    {
        var employee = _employees.FirstOrDefault(x =>
            x.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(employee);
    }

    public Task AddAsync(Employee employee)
    {
        _employees.Add(employee);
        return Task.CompletedTask;
    }
}
