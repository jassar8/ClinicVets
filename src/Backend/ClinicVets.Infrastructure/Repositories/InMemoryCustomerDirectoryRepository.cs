using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

/// <summary>In-memory customer/animal directory for desktop quick-access demo (no JSON writes).</summary>
public sealed class InMemoryCustomerDirectoryRepository : ICustomerDirectoryRepository
{
    private readonly object _sync = new();
    private readonly List<Customer> _customers;
    private readonly List<Animal> _animals;

    public InMemoryCustomerDirectoryRepository(IEnumerable<Customer> customers, IEnumerable<Animal> animals)
    {
        _customers = customers.Select(c => new Customer
        {
            Id = c.Id,
            FullName = c.FullName,
            NationalId = c.NationalId,
            Phone = c.Phone,
            Email = c.Email
        }).ToList();
        _animals = animals.Select(CloneAnimal).ToList();
        RefreshOwnerIds();
    }

    public Task<IReadOnlyList<Customer>> GetAllCustomersAsync()
    {
        lock (_sync)
        {
            var list = _customers
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
            var match = _customers.FirstOrDefault(c =>
                string.Equals(c.NationalId?.Trim(), key, StringComparison.Ordinal));
            return Task.FromResult(match);
        }
    }

    public Task AddCustomerAsync(Customer customer)
    {
        lock (_sync)
        {
            _customers.Add(new Customer
            {
                Id = customer.Id,
                FullName = customer.FullName,
                NationalId = customer.NationalId,
                Phone = customer.Phone,
                Email = customer.Email
            });
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
                var all = _customers
                    .OrderBy(c => c.FullName, StringComparer.OrdinalIgnoreCase)
                    .ToList();
                return Task.FromResult<IReadOnlyList<Customer>>(all);
            }

            var tokens = raw.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            IEnumerable<Customer> q = _customers;
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
            var list = _animals
                .Where(a => a.CustomerId == customerId)
                .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
            return Task.FromResult<IReadOnlyList<Animal>>(list);
        }
    }

    public Task<IReadOnlyList<Animal>> GetAllAnimalsAsync()
    {
        lock (_sync)
            return Task.FromResult<IReadOnlyList<Animal>>(_animals.Select(CloneAnimal).ToList());
    }

    public Task<Animal?> GetAnimalByChipNumberAsync(string chipNumber)
    {
        var key = chipNumber.Trim();
        lock (_sync)
            return Task.FromResult(_animals.FirstOrDefault(a =>
                string.Equals(a.ChipNumber, key, StringComparison.OrdinalIgnoreCase)) is { } a
                ? CloneAnimal(a)
                : null);
    }

    public Task AddAnimalAsync(Animal animal)
    {
        lock (_sync)
        {
            if (string.IsNullOrWhiteSpace(animal.ChipNumber))
                animal.ChipNumber = GenerateChipNumber();
            RefreshOwnerId(animal);
            _animals.Add(CloneAnimal(animal));
        }

        return Task.CompletedTask;
    }

    public Task UpdateAnimalAsync(Animal animal)
    {
        lock (_sync)
        {
            var index = _animals.FindIndex(a => a.Id == animal.Id);
            if (index >= 0)
            {
                RefreshOwnerId(animal);
                _animals[index] = CloneAnimal(animal);
            }
        }

        return Task.CompletedTask;
    }

    private void RefreshOwnerIds()
    {
        foreach (var animal in _animals)
            RefreshOwnerId(animal);
    }

    private void RefreshOwnerId(Animal animal)
    {
        var owner = _customers.FirstOrDefault(c => c.Id == animal.CustomerId);
        animal.OwnerIdNumber = owner?.NationalId ?? string.Empty;
    }

    private static Animal CloneAnimal(Animal a) => new()
    {
        Id = a.Id,
        CustomerId = a.CustomerId,
        Name = a.Name,
        Species = a.Species,
        ChipNumber = a.ChipNumber,
        Weight = a.Weight,
        BirthDate = a.BirthDate,
        LastVaccinationDate = a.LastVaccinationDate,
        OwnerIdNumber = a.OwnerIdNumber
    };

    private static string GenerateChipNumber() =>
        "376" + Random.Shared.Next(1000, 9999).ToString();
}
