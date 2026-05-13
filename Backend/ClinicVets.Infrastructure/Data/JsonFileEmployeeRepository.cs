using System.Diagnostics;
using System.Text.Json;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Validation;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Data;

/// <summary>
/// Persists employees to a JSON file under the user's local app data (desktop demo, no database).
/// </summary>
public sealed class JsonFileEmployeeRepository : IEmployeeRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _sync = new();
    private readonly string _filePath;
    private List<Employee> _employees;

    /// <param name="persistenceRootOverride">
    /// Optional folder that will contain <c>employees.json</c> (for tests). When null, uses LocalApplicationData\ClinicVets.
    /// </param>
    public JsonFileEmployeeRepository(string? persistenceRootOverride = null)
    {
        var dir = string.IsNullOrWhiteSpace(persistenceRootOverride)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClinicVets")
            : persistenceRootOverride.Trim();
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "employees.json");
        _employees = LoadOrSeed();
        LogBootstrapState();
    }

    private void LogBootstrapState()
    {
        try
        {
            lock (_sync)
            {
                var probe = EmployeeLoginLookup.FindEmployee(_employees, SystemAccounts.DefaultAdminUsername);
                Trace.WriteLine($"[ClinicVets] Employee store: {_filePath}");
                Trace.WriteLine($"[ClinicVets] Employee count: {_employees.Count}");
                Trace.WriteLine(
                    probe is null
                        ? "[ClinicVets] Bootstrap admin lookup(admin): FAILED"
                        : $"[ClinicVets] Bootstrap admin lookup(admin): OK role={probe.Role} email={probe.Email} username={probe.Username} passwordLength={probe.Password?.Length ?? 0}");
            }
        }
        catch (Exception ex)
        {
            Trace.WriteLine("[ClinicVets] Bootstrap log error: " + ex.Message);
        }
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
                .Where(e => string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
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
            SaveUnlocked();
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Employee employee)
    {
        lock (_sync)
        {
            var idx = _employees.FindIndex(e => e.Id == employee.Id);
            if (idx >= 0)
            {
                _employees[idx] = employee;
                SaveUnlocked();
            }
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
            var removed = _employees.RemoveAll(e =>
                string.Equals(e.Email?.Trim(), key, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Rejected, StringComparison.OrdinalIgnoreCase));
            if (removed > 0)
                SaveUnlocked();
        }

        return Task.CompletedTask;
    }

    private List<Employee> LoadOrSeed()
    {
        lock (_sync)
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    var list = JsonSerializer.Deserialize<List<Employee>>(json, JsonOptions);
                    if (list is { Count: > 0 })
                    {
                        _employees = list;
                        ApplyCanonicalBootstrapAdministratorUnlocked();
                        NormalizeLegacyEmployeeRecordsUnlocked();
                        SaveUnlocked();
                        return _employees;
                    }
                }
                catch
                {
                    // Fall through to seed defaults if file is corrupt.
                }
            }

            var seed = CreateDefaultEmployees();
            _employees = seed;
            ApplyCanonicalBootstrapAdministratorUnlocked();
            NormalizeLegacyEmployeeRecordsUnlocked();
            SaveUnlocked();
            return _employees;
        }
    }

    private void NormalizeLegacyEmployeeRecordsUnlocked()
    {
        foreach (var e in _employees)
        {
            if (string.IsNullOrWhiteSpace(e.Status))
                e.Status = EmployeeAccountStatusNames.Approved;
        }

        foreach (var e in _employees)
        {
            if (string.IsNullOrWhiteSpace(e.RequestedRole))
                e.RequestedRole = string.IsNullOrWhiteSpace(e.Role) ? string.Empty : e.Role.Trim();
        }

        foreach (var e in _employees.Where(x => RolePermissions.IsAdministrator(x)))
        {
            e.Status = EmployeeAccountStatusNames.Approved;
            if (!EmployeeIdValidation.IsFourDigitEmployeeId(e.EmployeeId))
                e.EmployeeId = "9000";
        }

        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in _employees)
        {
            if (EmployeeIdValidation.IsFourDigitEmployeeId(e.EmployeeId))
                used.Add(e.EmployeeId.Trim());
        }

        foreach (var e in _employees)
        {
            if (!string.Equals(e.Status?.Trim(), EmployeeAccountStatusNames.Approved, StringComparison.OrdinalIgnoreCase))
                continue;
            if (RolePermissions.IsAdministrator(e))
                continue;
            if (EmployeeIdValidation.IsFourDigitEmployeeId(e.EmployeeId))
                continue;

            for (var n = 3001; n <= 9999; n++)
            {
                var id = n.ToString("D4");
                if (used.Add(id))
                {
                    e.EmployeeId = id;
                    break;
                }
            }
        }
    }

    /// <summary>
    /// Guarantees exactly one bootstrap row for <see cref="SystemAccounts.DefaultAdminEmail"/> with published demo credentials.
    /// </summary>
    private void ApplyCanonicalBootstrapAdministratorUnlocked()
    {
        _employees.RemoveAll(e =>
            string.Equals(e.Email?.Trim(), SystemAccounts.DefaultAdminEmail, StringComparison.OrdinalIgnoreCase));

        _employees.Insert(0, CreateDefaultAdmin());
    }

    private static List<Employee> CreateDefaultEmployees() =>
    [
        CreateDefaultAdmin(),
        new Employee
        {
            FullName = "Dr. Amir Levi",
            Email = "vet@clinicvets.com",
            Password = "Vet12!ab",
            Role = EmployeeRoleNames.Veterinarian,
            RequestedRole = EmployeeRoleNames.Veterinarian,
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "2001"
        },
        new Employee
        {
            FullName = "Maya Cohen",
            Email = "secretary@clinicvets.com",
            Password = "Sec12!ab",
            Role = EmployeeRoleNames.Secretary,
            RequestedRole = EmployeeRoleNames.Secretary,
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "2002"
        }
    ];

    private static Employee CreateDefaultAdmin() => new()
    {
        FullName = SystemAccounts.DefaultAdminDisplayName,
        Username = SystemAccounts.DefaultAdminUsername,
        Email = SystemAccounts.DefaultAdminEmail,
        Password = SystemAccounts.DefaultAdminPassword,
        Role = SystemAccounts.DefaultAdminRole,
        RequestedRole = SystemAccounts.DefaultAdminRole,
        Status = EmployeeAccountStatusNames.Approved,
        EmployeeId = "9000"
    };

    private void SaveUnlocked()
    {
        var json = JsonSerializer.Serialize(_employees, JsonOptions);
        File.WriteAllText(_filePath, json);
    }
}
