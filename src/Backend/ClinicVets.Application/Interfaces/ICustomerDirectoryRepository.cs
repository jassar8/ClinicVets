using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Interfaces;

public interface ICustomerDirectoryRepository
{
    Task<IReadOnlyList<Customer>> GetAllCustomersAsync();
    Task<Customer?> GetByNationalIdAsync(string nineDigitId);
    Task AddCustomerAsync(Customer customer);
    Task<IReadOnlyList<Customer>> SearchCustomersAsync(string query);
    Task<IReadOnlyList<Animal>> GetAnimalsByCustomerIdAsync(Guid customerId);
    Task AddAnimalAsync(Animal animal);
}
