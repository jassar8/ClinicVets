using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

public sealed class InMemoryMedicationRepository : IMedicationRepository
{
    private readonly List<Medication> _items;
    private readonly object _sync = new();

    public InMemoryMedicationRepository(IEnumerable<Medication> seed)
    {
        _items = seed.Select(Clone).ToList();
    }

    public Task<IReadOnlyList<Medication>> GetAllAsync()
    {
        lock (_sync)
            return Task.FromResult<IReadOnlyList<Medication>>(_items.Select(Clone).OrderBy(m => m.Name, StringComparer.OrdinalIgnoreCase).ToList());
    }

    public Task<Medication?> GetByIdAsync(int id)
    {
        lock (_sync)
            return Task.FromResult(_items.FirstOrDefault(m => m.Id == id) is { } m ? Clone(m) : null);
    }

    public Task<Medication?> GetByNameAsync(string name)
    {
        var key = name.Trim();
        if (key.Length == 0)
            return Task.FromResult<Medication?>(null);

        lock (_sync)
            return Task.FromResult(_items.FirstOrDefault(m =>
                string.Equals(m.Name, key, StringComparison.OrdinalIgnoreCase)) is { } m ? Clone(m) : null);
    }

    public Task AddAsync(Medication medication)
    {
        lock (_sync)
        {
            medication.Id = _items.Count == 0 ? 1 : _items.Max(m => m.Id) + 1;
            _items.Add(Clone(medication));
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Medication medication)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(m => m.Id == medication.Id);
            if (index >= 0)
                _items[index] = Clone(medication);
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
            return Task.FromResult(true);
        }
    }

    private static Medication Clone(Medication source) => new()
    {
        Id = source.Id,
        Name = source.Name,
        StockQuantity = source.StockQuantity,
        UnitPrice = source.UnitPrice,
        ExpirationDate = source.ExpirationDate,
        Notes = source.Notes
    };
}
