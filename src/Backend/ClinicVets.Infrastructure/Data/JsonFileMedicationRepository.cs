using System.Text.Json;
using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Data;

/// <summary>Persists medicine inventory to JSON under local app data (same store folder as employees).</summary>
public sealed class JsonFileMedicationRepository : IMedicationRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _sync = new();
    private readonly string _filePath;
    private List<Medication> _items;

    public JsonFileMedicationRepository(string? persistenceRootOverride = null)
    {
        var dir = string.IsNullOrWhiteSpace(persistenceRootOverride)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClinicVets")
            : persistenceRootOverride.Trim();
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "medications.json");
        _items = LoadOrSeed();
    }

    public Task<IReadOnlyList<Medication>> GetAllAsync()
    {
        lock (_sync)
        {
            return Task.FromResult<IReadOnlyList<Medication>>(_items.OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList());
        }
    }

    public Task<Medication?> GetByIdAsync(int id)
    {
        lock (_sync)
        {
            return Task.FromResult(_items.FirstOrDefault(m => m.Id == id));
        }
    }

    public Task<Medication?> GetByNameAsync(string name)
    {
        var key = name.Trim();
        if (key.Length == 0)
            return Task.FromResult<Medication?>(null);

        lock (_sync)
        {
            return Task.FromResult(_items.FirstOrDefault(m =>
                string.Equals(m.Name, key, StringComparison.OrdinalIgnoreCase)));
        }
    }

    public Task AddAsync(Medication medication)
    {
        lock (_sync)
        {
            medication.Id = _items.Count == 0 ? 1 : _items.Max(m => m.Id) + 1;
            _items.Add(medication);
            SaveLocked();
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Medication medication)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(m => m.Id == medication.Id);
            if (index < 0)
                return Task.CompletedTask;
            _items[index] = medication;
            SaveLocked();
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(int id)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(m => m.Id == id);
            if (index < 0)
                return Task.FromResult(false);
            _items.RemoveAt(index);
            SaveLocked();
            return Task.FromResult(true);
        }
    }

    private List<Medication> LoadOrSeed()
    {
        if (!File.Exists(_filePath))
        {
            var seed = CreateSeedItems();
            File.WriteAllText(_filePath, JsonSerializer.Serialize(seed, JsonOptions));
            return seed;
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var loaded = JsonSerializer.Deserialize<List<Medication>>(json, JsonOptions);
            return loaded is { Count: > 0 } ? loaded : CreateSeedItems();
        }
        catch
        {
            return CreateSeedItems();
        }
    }

    private void SaveLocked() =>
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_items, JsonOptions));

    private static List<Medication> CreateSeedItems() =>
    [
        new Medication
        {
            Id = 1,
            Name = "Amoxicillin 250mg",
            StockQuantity = 42,
            UnitPrice = 12.5,
            ExpirationDate = DateTime.Today.AddMonths(8),
            Notes = "Antibiotic tablets"
        },
        new Medication
        {
            Id = 2,
            Name = "Rimadyl 100mg",
            StockQuantity = 4,
            UnitPrice = 28.0,
            ExpirationDate = DateTime.Today.AddDays(20),
            Notes = "Anti-inflammatory"
        },
        new Medication
        {
            Id = 3,
            Name = "Frontline Plus",
            StockQuantity = 18,
            UnitPrice = 35.75,
            ExpirationDate = DateTime.Today.AddYears(1),
            Notes = "Flea and tick treatment"
        }
    ];
}
