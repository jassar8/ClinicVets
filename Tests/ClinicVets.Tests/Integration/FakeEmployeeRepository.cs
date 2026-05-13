using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;

namespace ClinicVets.Tests.Integration;

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
            string.Equals(e.Email?.Trim(), key, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(match);
    }

    public Task<Employee?> GetByLoginIdentifierAsync(string loginIdentifier)
    {
        var raw = loginIdentifier?.Trim() ?? string.Empty;
        if (raw.Length == 0)
            return Task.FromResult<Employee?>(null);

        return Task.FromResult(EmployeeLoginLookup.FindEmployee(_employees, raw));
    }

    public Task<IReadOnlyList<Employee>> GetAllAsync()
    {
        var copy = _employees
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult<IReadOnlyList<Employee>>(copy);
    }

    public Task AddAsync(Employee employee)
    {
        _employees.Add(employee);
        return Task.CompletedTask;
    }
}
