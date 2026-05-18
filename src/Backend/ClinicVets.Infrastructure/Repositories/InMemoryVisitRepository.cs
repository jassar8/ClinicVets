using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

public sealed class InMemoryVisitRepository : IVisitRepository
{
    private readonly List<Visit> _items;
    private readonly object _sync = new();

    public InMemoryVisitRepository(IEnumerable<Visit> seed) =>
        _items = seed.Select(Clone).ToList();

    public Task<IReadOnlyList<Visit>> GetAllAsync()
    {
        lock (_sync)
            return Task.FromResult<IReadOnlyList<Visit>>(_items.Select(Clone).ToList());
    }

    public Task<Visit?> GetByIdAsync(int id)
    {
        lock (_sync)
            return Task.FromResult(_items.FirstOrDefault(v => v.Id == id) is { } v ? Clone(v) : null);
    }

    public Task AddAsync(Visit visit)
    {
        lock (_sync)
        {
            visit.Id = _items.Count == 0 ? 1 : _items.Max(v => v.Id) + 1;
            _items.Add(Clone(visit));
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Visit visit)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(v => v.Id == visit.Id);
            if (index >= 0)
                _items[index] = Clone(visit);
        }

        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(int id)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(v => v.Id == id);
            if (index < 0)
                return Task.FromResult(false);
            _items.RemoveAt(index);
            return Task.FromResult(true);
        }
    }

    public Task SaveAllAsync(IReadOnlyList<Visit> visits)
    {
        lock (_sync)
        {
            _items.Clear();
            _items.AddRange(visits.Select(Clone));
        }

        return Task.CompletedTask;
    }

    private static Visit Clone(Visit v) => new()
    {
        Id = v.Id,
        AnimalChipNumber = v.AnimalChipNumber,
        VisitDate = v.VisitDate,
        Reason = v.Reason,
        Symptoms = v.Symptoms,
        Diagnosis = v.Diagnosis,
        VeterinarianName = v.VeterinarianName,
        BaseCost = v.BaseCost,
        MedicationName = v.MedicationName,
        MedicationQuantity = v.MedicationQuantity,
        TotalCost = v.TotalCost,
        ArrivalStatus = v.ArrivalStatus,
        ArrivalNote = v.ArrivalNote
    };
}
