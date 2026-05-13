using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByEmailAsync(string email);
    Task<Employee?> GetByLoginIdentifierAsync(string loginIdentifier);
    Task<IReadOnlyList<Employee>> GetAllAsync();
    Task AddAsync(Employee employee);
}
