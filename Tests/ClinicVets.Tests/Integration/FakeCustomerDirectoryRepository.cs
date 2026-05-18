using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Tests.Integration;

public sealed class FakeCustomerDirectoryRepository : ICustomerDirectoryRepository
{
    private readonly List<Customer> _customers = [];
    private readonly List<Animal> _animals = [];

    public Task<IReadOnlyList<Customer>> GetAllCustomersAsync()
    {
        var copy = _customers.OrderBy(c => c.FullName, StringComparer.OrdinalIgnoreCase).ToList();
        return Task.FromResult<IReadOnlyList<Customer>>(copy);
    }

    public Task<Customer?> GetByNationalIdAsync(string nineDigitId)
    {
        var key = nineDigitId.Trim();
        var m = _customers.FirstOrDefault(c =>
            string.Equals(c.NationalId?.Trim(), key, StringComparison.Ordinal));
        return Task.FromResult(m);
    }

    public Task AddCustomerAsync(Customer customer)
    {
        _customers.Add(customer);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<Customer>> SearchCustomersAsync(string query)
    {
        var raw = query?.Trim() ?? string.Empty;
        if (raw.Length == 0)
            return GetAllCustomersAsync();

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

    public Task<IReadOnlyList<Animal>> GetAnimalsByCustomerIdAsync(Guid customerId)
    {
        var list = _animals
            .Where(a => a.CustomerId == customerId)
            .OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
        return Task.FromResult<IReadOnlyList<Animal>>(list);
    }

    public Task<IReadOnlyList<Animal>> GetAllAnimalsAsync()
    {
        var list = _animals.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList();
        return Task.FromResult<IReadOnlyList<Animal>>(list);
    }

    public Task<Animal?> GetAnimalByChipNumberAsync(string chipNumber)
    {
        var key = chipNumber.Trim();
        var m = _animals.FirstOrDefault(a =>
            string.Equals(a.ChipNumber?.Trim(), key, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(m);
    }

    public Task UpdateAnimalAsync(Animal animal)
    {
        var index = _animals.FindIndex(a => a.Id == animal.Id);
        if (index >= 0)
            _animals[index] = animal;
        return Task.CompletedTask;
    }

    public Task AddAnimalAsync(Animal animal)
    {
        _animals.Add(animal);
        return Task.CompletedTask;
    }
}
