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

    public Task<Employee?> GetByIdAsync(Guid id)
    {
        var match = _employees.FirstOrDefault(e => e.Id == id);
        return Task.FromResult(match);
    }

    public Task<IReadOnlyList<Employee>> GetAllAsync()
    {
        var copy = _employees
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult<IReadOnlyList<Employee>>(copy);
    }

    public Task<IReadOnlyList<Employee>> GetPendingRegistrationsAsync()
    {
        var list = _employees
            .Where(e =>
                string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
            .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult<IReadOnlyList<Employee>>(list);
    }

    public Task AddAsync(Employee employee)
    {
        _employees.Add(employee);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Employee employee)
    {
        var idx = _employees.FindIndex(e => e.Id == employee.Id);
        if (idx >= 0)
            _employees[idx] = employee;
        return Task.CompletedTask;
    }

    public Task RemoveRejectedApplicationsForEmailAsync(string normalizedEmail)
    {
        var key = normalizedEmail.Trim();
        if (key.Length == 0)
            return Task.CompletedTask;

        _employees.RemoveAll(e =>
            string.Equals(e.Email?.Trim(), key, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Rejected, StringComparison.OrdinalIgnoreCase));
        return Task.CompletedTask;
    }

    public Task<bool> DeleteRejectedEmployeeAsync(Guid id)
    {
        var idx = _employees.FindIndex(e =>
            e.Id == id &&
            string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Rejected, StringComparison.OrdinalIgnoreCase));
        if (idx < 0)
            return Task.FromResult(false);

        _employees.RemoveAt(idx);
        return Task.FromResult(true);
    }
}
