using System.Diagnostics;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Validation;
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

        if (RolePermissions.IsAdministrator(employee))
        {
            Trace.WriteLine($"[ClinicVets] Login OK (administrator): '{id}' role={employee.Role}.");
            return (true, "Login successful.", employee);
        }

        var status = employee.Status?.Trim() ?? string.Empty;
        if (string.Equals(status, EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
        {
            Trace.WriteLine($"[ClinicVets] Login blocked (pending): '{id}'.");
            return (false, "Your account is waiting for admin approval.", null);
        }

        if (string.Equals(status, EmployeeAccountStatusNames.Rejected, StringComparison.OrdinalIgnoreCase))
        {
            Trace.WriteLine($"[ClinicVets] Login blocked (rejected): '{id}'.");
            return (false, "Your account request was rejected.", null);
        }

        if (!string.Equals(status, EmployeeAccountStatusNames.Approved, StringComparison.OrdinalIgnoreCase) ||
            !EmployeeIdValidation.IsFourDigitEmployeeId(employee.EmployeeId))
        {
            Trace.WriteLine($"[ClinicVets] Login blocked (not active): '{id}' status={status}.");
            return (false, "Your account is waiting for admin approval.", null);
        }

        Trace.WriteLine($"[ClinicVets] Login OK: '{id}' role={employee.Role}.");
        return (true, "Login successful.", employee);
    }
}
