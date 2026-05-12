using ClinicVets.Application.Interfaces;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

public class EmployeeAuthenticationService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeAuthenticationService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    public async Task<(bool IsSuccess, string Message, Employee? Employee)> LoginAsync(string email, string password)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Email and password are required.", null);
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var employee = await _employeeRepository.GetByEmailAsync(normalizedEmail);
        if (employee is null || !string.Equals(employee.Password, password.Trim(), StringComparison.Ordinal))
        {
            return (false, "Invalid email or password.", null);
        }

        return (true, "Login successful.", employee);
    }
}
