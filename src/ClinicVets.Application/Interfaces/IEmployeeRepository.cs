using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Interfaces;

public interface IEmployeeRepository
{
    Task<Employee?> GetByEmailAsync(string email);
    Task AddAsync(Employee employee);
}
