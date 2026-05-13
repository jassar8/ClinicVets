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

    public async Task<(bool IsSuccess, string Message, Employee? Employee)> LoginAsync(string loginIdentifier, string password)
    {
        if (string.IsNullOrWhiteSpace(loginIdentifier) || string.IsNullOrWhiteSpace(password))
        {
            return (false, "Sign-in name and password are required.", null);
        }

        var id = loginIdentifier.Trim();
        var employee = await _employeeRepository.GetByLoginIdentifierAsync(id);
        var stored = (employee?.Password ?? string.Empty).Trim();
        var provided = password.Trim();
        if (employee is null || !string.Equals(stored, provided, StringComparison.Ordinal))
        {
            return (false, "Invalid sign-in name or password.", null);
        }

        return (true, "Login successful.", employee);
    }
}
