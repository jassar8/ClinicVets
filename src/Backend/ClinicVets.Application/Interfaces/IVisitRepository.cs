using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Interfaces;

public interface IVisitRepository
{
    Task<IReadOnlyList<Visit>> GetAllAsync();
    Task<Visit?> GetByIdAsync(int id);
    Task AddAsync(Visit visit);
    Task UpdateAsync(Visit visit);
    Task<bool> DeleteAsync(int id);
    Task SaveAllAsync(IReadOnlyList<Visit> visits);
}
