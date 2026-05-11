using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

public class EmployeeRegistrationService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeRegistrationService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<(bool IsSuccess, string Message)> RegisterAsync(
        string fullName,
        string email,
        string password,
        string role)
    {
        // Basic validation kept in the service for clear demo flow and backend safety.
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(role))
        {
            return (false, "All fields are required.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existingEmployee = await _employeeRepository.GetByEmailAsync(normalizedEmail);
        if (existingEmployee is not null)
        {
            return (false, "An employee with this email already exists.");
        }

        var employee = new Employee
        {
            FullName = fullName.Trim(),
            Email = normalizedEmail,
            Password = password.Trim(),
            Role = role.Trim()
        };

        await _employeeRepository.AddAsync(employee);
        return (true, "Employee registered successfully.");
    }
}
