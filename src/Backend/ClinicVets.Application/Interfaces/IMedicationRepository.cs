using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Interfaces;

public interface IMedicationRepository
{
    Task<IReadOnlyList<Medication>> GetAllAsync();
    Task<Medication?> GetByIdAsync(int id);
    Task<Medication?> GetByNameAsync(string name);
    Task AddAsync(Medication medication);
    Task UpdateAsync(Medication medication);
    Task<bool> DeleteAsync(int id);
}
