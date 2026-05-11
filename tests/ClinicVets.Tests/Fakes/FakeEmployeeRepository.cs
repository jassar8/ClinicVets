using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Tests.Fakes;

/// <summary>
/// In-memory repository for unit tests only (not used by the desktop app).
/// </summary>
public sealed class FakeEmployeeRepository : IEmployeeRepository
{
    private readonly List<Employee> _employees = [];

    public Task<Employee?> GetByEmailAsync(string email)
    {
        var key = email?.Trim() ?? string.Empty;
        var match = _employees.FirstOrDefault(e =>
            e.Email.Equals(key, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match);
    }

    public Task AddAsync(Employee employee)
    {
        _employees.Add(employee);
        return Task.CompletedTask;
    }
}
