using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

public class InMemoryEmployeeRepository : IEmployeeRepository
{
    private readonly List<Employee> _employees =
    [
        new Employee
        {
            FullName = "Demo Vet Admin",
            Email = "admin@clinicvets.com",
            Password = "Admin123!",
            Role = "Administrator"
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
