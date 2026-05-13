using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByLoginIdentifierAsync(string loginIdentifier);
    Task<Employee?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<Employee>> GetAllAsync();
    Task<IReadOnlyList<Employee>> GetPendingRegistrationsAsync();
    Task AddAsync(Employee employee);
    Task UpdateAsync(Employee employee);
    Task RemoveRejectedApplicationsForEmailAsync(string normalizedEmail);
}
