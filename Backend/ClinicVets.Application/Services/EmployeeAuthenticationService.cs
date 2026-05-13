using System.Diagnostics;
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
        var provided = password.Trim();
        var employee = await _employeeRepository.GetByLoginIdentifierAsync(id);
        if (employee is null)
        {
            Trace.WriteLine($"[ClinicVets] Login failed: no employee for sign-in '{id}'.");
            return (false, "Invalid sign-in name or password.", null);
        }

        var stored = (employee.Password ?? string.Empty).Trim();
        if (!string.Equals(stored, provided, StringComparison.Ordinal))
        {
            Trace.WriteLine(
                $"[ClinicVets] Login failed: password mismatch for '{id}' (stored length {stored.Length}, provided length {provided.Length}).");
            return (false, "Invalid sign-in name or password.", null);
        }

        Trace.WriteLine($"[ClinicVets] Login OK: '{id}' role={employee.Role}.");
        return (true, "Login successful.", employee);
    }
}
