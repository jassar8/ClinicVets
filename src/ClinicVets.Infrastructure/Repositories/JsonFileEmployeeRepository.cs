using System.Text.Json;
using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

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
                        return list;
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

    private static List<Employee> CreateDefaultEmployees() =>
    [
        new Employee
        {
            FullName = "Dr. Amir Levi",
            Email = "vet@clinicvets.com",
            Password = "Vet12!ab",
            Role = "Veterinarian"
        },
        new Employee
        {
            FullName = "Maya Cohen",
            Email = "secretary@clinicvets.com",
            Password = "Sec12!ab",
            Role = "Secretary"
        }
    ];

    private void SaveUnlocked()
    {
        var json = JsonSerializer.Serialize(_employees, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_filePath, json);
    }
}
