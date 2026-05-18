using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

/// <summary>In-memory employee store for desktop quick-access demo (no JSON writes).</summary>
public sealed class InMemoryEmployeeRepository : IEmployeeRepository
{
    private readonly object _sync = new();
    private readonly List<Employee> _employees;

    public InMemoryEmployeeRepository(IEnumerable<Employee> seed)
    {
        _employees = seed.ToList();
    }

    public Task<Employee?> GetByEmailAsync(string email)
    {
        var key = email?.Trim() ?? string.Empty;
        if (key.Length == 0)
            return Task.FromResult<Employee?>(null);

        lock (_sync)
        {
            var match = _employees.FirstOrDefault(e =>
                string.Equals(e.Email?.Trim(), key, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(match);
        }
    }

    public Task<Employee?> GetByLoginIdentifierAsync(string loginIdentifier)
    {
        var raw = loginIdentifier?.Trim() ?? string.Empty;
        if (raw.Length == 0)
            return Task.FromResult<Employee?>(null);

        lock (_sync)
        {
            return Task.FromResult(EmployeeLoginLookup.FindEmployee(_employees, raw));
        }
    }

    public Task<Employee?> GetByIdAsync(Guid id)
    {
        lock (_sync)
        {
            var match = _employees.FirstOrDefault(e => e.Id == id);
            return Task.FromResult(match);
        }
    }

    public Task<IReadOnlyList<Employee>> GetAllAsync()
    {
        lock (_sync)
        {
            var copy = _employees
                .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return Task.FromResult<IReadOnlyList<Employee>>(copy);
        }
    }

    public Task<IReadOnlyList<Employee>> GetPendingRegistrationsAsync()
    {
        lock (_sync)
        {
            var list = _employees
                .Where(e =>
                    string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
                .OrderBy(e => e.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return Task.FromResult<IReadOnlyList<Employee>>(list);
        }
    }

    public Task AddAsync(Employee employee)
    {
        lock (_sync)
        {
            _employees.Add(employee);
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Employee employee)
    {
        lock (_sync)
        {
            var idx = _employees.FindIndex(e => e.Id == employee.Id);
            if (idx >= 0)
                _employees[idx] = employee;
        }

        return Task.CompletedTask;
    }

    public Task RemoveRejectedApplicationsForEmailAsync(string normalizedEmail)
    {
        var key = normalizedEmail.Trim();
        if (key.Length == 0)
            return Task.CompletedTask;

        lock (_sync)
        {
            _employees.RemoveAll(e =>
                string.Equals(e.Email?.Trim(), key, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Rejected, StringComparison.OrdinalIgnoreCase));
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteRejectedEmployeeAsync(Guid id)
    {
        lock (_sync)
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
}
