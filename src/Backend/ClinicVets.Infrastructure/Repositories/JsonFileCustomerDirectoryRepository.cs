using System.Text.Json;
using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

/// <summary>
/// Persists customers and their animals to JSON under local app data.
/// </summary>
public sealed class JsonFileCustomerDirectoryRepository : ICustomerDirectoryRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _sync = new();
    private readonly string _filePath;
    private CustomerDirectoryDocument _store;

    public JsonFileCustomerDirectoryRepository(string? persistenceRootOverride = null)
    {
        var dir = string.IsNullOrWhiteSpace(persistenceRootOverride)
            ? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ClinicVets")
            : persistenceRootOverride.Trim();
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "customer-directory.json");
        _store = LoadOrSeed();
    }

    public Task<IReadOnlyList<Customer>> GetAllCustomersAsync()
    {
        lock (_sync)
        {
            var list = _store.Customers
                .OrderBy(c => c.FullName, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return Task.FromResult<IReadOnlyList<Customer>>(list);
        }
    }

    public Task<Customer?> GetByNationalIdAsync(string nineDigitId)
    {
        var key = nineDigitId.Trim();
        if (key.Length != 9 || !key.All(char.IsDigit))
            return Task.FromResult<Customer?>(null);

        lock (_sync)
        {
            var match = _store.Customers.FirstOrDefault(c =>
                string.Equals(c.NationalId?.Trim(), key, StringComparison.Ordinal));
            return Task.FromResult(match);
        }
    }

    public Task AddCustomerAsync(Customer customer)
    {
        lock (_sync)
        {
            _store.Customers.Add(customer);
            SaveUnlocked();
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Customer>> SearchCustomersAsync(string query)
    {
        lock (_sync)
        {
            var raw = query?.Trim() ?? string.Empty;
            if (raw.Length == 0)
            {
                return GetAllCustomersAsync();
            }

            var tokens = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            IEnumerable<Customer> q = _store.Customers;
            foreach (var token in tokens)
            {
                var t = token;
                q = q.Where(c =>
                    c.FullName.Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    (c.Email ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    (c.Phone ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase) ||
                    (c.NationalId ?? string.Empty).Contains(t, StringComparison.OrdinalIgnoreCase));
            }

            var list = q.OrderBy(c => c.FullName, StringComparer.OrdinalIgnoreCase).ToList();
            return Task.FromResult<IReadOnlyList<Customer>>(list);
        }
    }

    public Task<IReadOnlyList<Animal>> GetAnimalsByCustomerIdAsync(Guid customerId)
    {
        lock (_sync)
        {
            var list = _store.Animals
                .Where(a => a.CustomerId == customerId)
                .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return Task.FromResult<IReadOnlyList<Animal>>(list);
        }
    }

    public Task<IReadOnlyList<Animal>> GetAllAnimalsAsync()
    {
        lock (_sync)
        {
            RefreshAnimalOwnerIds();
            var list = _store.Animals.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList();
            return Task.FromResult<IReadOnlyList<Animal>>(list);
        }
    }

    public Task<Animal?> GetAnimalByChipNumberAsync(string chipNumber)
    {
        var key = chipNumber.Trim();
        lock (_sync)
        {
            RefreshAnimalOwnerIds();
            var match = _store.Animals.FirstOrDefault(a =>
                string.Equals(a.ChipNumber, key, StringComparison.OrdinalIgnoreCase));
            return Task.FromResult(match);
        }
    }

    public Task AddAnimalAsync(Animal animal)
    {
        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(animal.ChipNumber))
                animal.ChipNumber = "376" + Random.Shared.Next(1000, 9999);
            RefreshAnimalOwnerId(animal);
            _store.Animals.Add(animal);
            SaveUnlocked();
        }

        return Task.CompletedTask;
    }

    public Task UpdateAnimalAsync(Animal animal)
    {
        lock (_sync)
        {
            var index = _store.Animals.FindIndex(a => a.Id == animal.Id);
            if (index >= 0)
            {
                RefreshAnimalOwnerId(animal);
                _store.Animals[index] = animal;
                SaveUnlocked();
            }
        }

        return Task.CompletedTask;
    }

    private void RefreshAnimalOwnerIds()
    {
        foreach (var animal in _store.Animals)
            RefreshAnimalOwnerId(animal);
    }

    private void RefreshAnimalOwnerId(Animal animal)
    {
        var owner = _store.Customers.FirstOrDefault(c => c.Id == animal.CustomerId);
        animal.OwnerIdNumber = owner?.NationalId ?? string.Empty;
    }

    private CustomerDirectoryDocument LoadOrSeed()
    {
        lock (_sync)
        {
            if (File.Exists(_filePath))
            {
                try
                {
                    var json = File.ReadAllText(_filePath);
                    var doc = JsonSerializer.Deserialize<CustomerDirectoryDocument>(json, JsonOptions);
                    if (doc?.Customers is { Count: > 0 })
                    {
                        doc.Animals ??= [];
                        return doc;
                    }
                }
                catch
                {
                    // fall through to seed
                }
            }

            var seed = CreateSeed();
            _store = seed;
            SaveUnlocked();
            return _store;
        }
    }

    private static CustomerDirectoryDocument CreateSeed()
    {
        var c1 = new Customer
        {
            FullName = "Dana Bar",
            NationalId = "300456789",
            Phone = "+972-52-555-0142",
            Email = "dana.bar@example.com"
        };
        var c2 = new Customer
        {
            FullName = "Eli Cohen",
            NationalId = "029876543",
            Phone = "054-321-9876",
            Email = "eli.cohen@example.com"
        };

        var animals = new List<Animal>
        {
            new()
            {
                CustomerId = c1.Id,
                Name = "Mitzi",
                Species = "חתול",
                ChipNumber = "3761001",
                Weight = 4.2,
                OwnerIdNumber = c1.NationalId
            },
            new()
            {
                CustomerId = c1.Id,
                Name = "Rex",
                Species = "כלב",
                ChipNumber = "3761002",
                Weight = 28.5,
                OwnerIdNumber = c1.NationalId
            },
            new()
            {
                CustomerId = c2.Id,
                Name = "Kiwi",
                Species = "ציפור",
                ChipNumber = "3762001",
                Weight = 0.3,
                OwnerIdNumber = c2.NationalId
            }
        };

        return new CustomerDirectoryDocument
        {
            Customers = [c1, c2],
            Animals = animals
        };
    }

    private void SaveUnlocked()
    {
        var json = JsonSerializer.Serialize(_store, JsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private sealed class CustomerDirectoryDocument
    {
        public List<Customer> Customers { get; set; } = [];
        public List<Animal> Animals { get; set; } = [];
    }
}
