using System.Text.Json;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Data;

/// <summary>
/// Persists employees to a JSON file under the user's local app data (desktop demo, no database).
/// </summary>
public sealed class JsonFileEmployeeRepository : IEmployeeRepository
{
    private readonly object _sync = new();
    private readonly string _filePath;
    private List<Employee> _employees;

    public JsonFileEmployeeRepository()
    {
        var dir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ClinicVets");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "employees.json");
        _employees = LoadOrSeed();
    }

    public Task<Employee?> GetByEmailAsync(string email)
    {
        var key = email?.Trim() ?? string.Empty;
        if (key.Length == 0)
            return Task.FromResult<Employee?>(null);

        lock (_sync)
        {
            var match = _employees.FirstOrDefault(e =>
                e.Email.Equals(key, StringComparison.OrdinalIgnoreCase));
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
            if (raw.Contains('@', StringComparison.Ordinal))
            {
                var key = raw.ToLowerInvariant();
                var match = _employees.FirstOrDefault(e =>
                    e.Email.Equals(key, StringComparison.OrdinalIgnoreCase));
                return Task.FromResult(match);
            }

            var matchByAlias = _employees.FirstOrDefault(e =>
                (!string.IsNullOrWhiteSpace(e.Username) &&
                 e.Username.Equals(raw, StringComparison.OrdinalIgnoreCase)) ||
                e.Email.Equals(raw, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(matchByAlias);
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

    public Task AddAsync(Employee employee)
    {
        lock (_sync)
        {
            _employees.Add(employee);
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
                    var list = JsonSerializer.Deserialize<List<Employee>>(json);
                    if (list is { Count: > 0 })
                    {
                        _employees = list;
                        EnsureBootstrapAdminIfMissingUnlocked();
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
            SaveUnlocked();
            return seed;
        }
    }

    private void EnsureBootstrapAdminIfMissingUnlocked()
    {
        var hasAdmin = _employees.Any(e => RolePermissions.IsAdministrator(e));
        if (hasAdmin)
            return;

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
            Role = EmployeeRoleNames.Veterinarian
        },
        new Employee
        {
            FullName = "Maya Cohen",
            Email = "secretary@clinicvets.com",
            Password = "Sec12!ab",
            Role = EmployeeRoleNames.Secretary
        }
    ];

    private static Employee CreateDefaultAdmin() => new()
    {
        FullName = SystemAccounts.DefaultAdminDisplayName,
        Username = SystemAccounts.DefaultAdminUsername,
        Email = SystemAccounts.DefaultAdminEmail,
        Password = SystemAccounts.DefaultAdminPassword,
        Role = SystemAccounts.DefaultAdminRole
    };

    private void SaveUnlocked()
    {
        var json = JsonSerializer.Serialize(_employees, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
