using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

public sealed class VisitManagementService
{
    private readonly IVisitRepository _repository;

    public VisitManagementService(IVisitRepository repository) => _repository = repository;

    public Task<IReadOnlyList<Visit>> GetAllAsync() => _repository.GetAllAsync();

    public Task SaveAllAsync(IReadOnlyList<Visit> visits) => _repository.SaveAllAsync(visits);

    public Task AddAsync(Visit visit) => _repository.AddAsync(visit);

    public Task UpdateAsync(Visit visit) => _repository.UpdateAsync(visit);

    public Task<bool> DeleteAsync(int id) => _repository.DeleteAsync(id);
}
