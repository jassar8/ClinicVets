using System.Text.Json;
using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Infrastructure.Repositories;

public sealed class JsonFileVisitRepository : IVisitRepository
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    private readonly object _sync = new();
    private readonly string _filePath;
    private List<Visit> _items;

    public JsonFileVisitRepository(string? persistenceRootOverride = null)
    {
        var dir = string.IsNullOrWhiteSpace(persistenceRootOverride)
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ClinicVets")
            : persistenceRootOverride.Trim();
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "visits.json");
        _items = LoadOrSeed();
    }

    public Task<IReadOnlyList<Visit>> GetAllAsync()
    {
        lock (_sync)
            return Task.FromResult<IReadOnlyList<Visit>>(_items.OrderBy(v => v.VisitDate).ToList());
    }

    public Task<Visit?> GetByIdAsync(int id)
    {
        lock (_sync)
            return Task.FromResult(_items.FirstOrDefault(v => v.Id == id));
    }

    public Task AddAsync(Visit visit)
    {
        lock (_sync)
        {
            visit.Id = _items.Count == 0 ? 1 : _items.Max(v => v.Id) + 1;
            _items.Add(Clone(visit));
            SaveLocked();
        }

        return Task.CompletedTask;
    }

    public Task UpdateAsync(Visit visit)
    {
        lock (_sync)
        {
            var index = _items.FindIndex(v => v.Id == visit.Id);
            if (index >= 0)
            {
                _items[index] = Clone(visit);
                SaveLocked();
            }
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
            SaveLocked();
            return Task.FromResult(true);
        }
    }

    public Task SaveAllAsync(IReadOnlyList<Visit> visits)
    {
        lock (_sync)
        {
            _items = visits.Select(Clone).ToList();
            SaveLocked();
        }

        return Task.CompletedTask;
    }

    private List<Visit> LoadOrSeed()
    {
        if (!File.Exists(_filePath))
            return [];

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<Visit>>(json, JsonOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private void SaveLocked() =>
        File.WriteAllText(_filePath, JsonSerializer.Serialize(_items, JsonOptions));

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
