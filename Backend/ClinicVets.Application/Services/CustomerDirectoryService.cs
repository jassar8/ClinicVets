using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Validation;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

public sealed class CustomerDirectoryService
{
    private readonly ICustomerDirectoryRepository _repository;

    public CustomerDirectoryService(ICustomerDirectoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<(bool Ok, string Message)> RegisterCustomerAsync(
        string fullName,
        string nationalId,
        string phone,
        string email)
    {
        if (!CustomerInputValidation.IsValidCustomerFullName(fullName))
        {
            return (false, "Full name must be 2–120 characters and contain letters only (spaces, hyphens, and apostrophes are allowed).");
        }

        if (!CustomerInputValidation.IsValidNationalId(nationalId))
        {
            return (false, "National ID must be exactly 9 digits.");
        }

        if (!CustomerInputValidation.IsValidCustomerPhone(phone))
        {
            return (false, "Phone must include 9–15 digits (spaces, dashes, and parentheses are allowed).");
        }

        if (!CustomerInputValidation.IsValidCustomerEmail(email))
        {
            return (false, "Please enter a valid email address.");
        }

        var idKey = nationalId.Trim();
        var existing = await _repository.GetByNationalIdAsync(idKey);
        if (existing is not null)
            return (false, "A customer with this national ID is already registered.");

        var customer = new Customer
        {
            FullName = fullName.Trim(),
            NationalId = idKey,
            Phone = phone.Trim(),
            Email = email.Trim().ToLowerInvariant()
        };

        await _repository.AddCustomerAsync(customer);
        return (true, "Customer registered successfully.");
    }

    public Task<IReadOnlyList<Customer>> SearchCustomersAsync(string query) =>
        _repository.SearchCustomersAsync(query);

    public Task<IReadOnlyList<Customer>> ListCustomersAsync() =>
        _repository.GetAllCustomersAsync();

    public Task<IReadOnlyList<Animal>> GetAnimalsForCustomerAsync(Guid customerId) =>
        _repository.GetAnimalsByCustomerIdAsync(customerId);
}
