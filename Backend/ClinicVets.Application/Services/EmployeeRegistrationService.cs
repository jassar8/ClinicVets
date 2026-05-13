using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Validation;
using ClinicVets.Core;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

public class EmployeeRegistrationService
{
    private readonly IEmployeeRepository _employeeRepository;

    public EmployeeRegistrationService(IEmployeeRepository employeeRepository)
    {
        _employeeRepository = employeeRepository;
    }

    /// <param name="actingEmployee">
    /// When null, only self-service roles (secretary / veterinarian) may be created.
    /// When set, must be an administrator to create any role including additional admins.
    /// </param>
    public async Task<(bool IsSuccess, string Message)> RegisterAsync(
        string fullName,
        string email,
        string password,
        string role,
        Employee? actingEmployee = null,
        string? username = null)
    {
        // Basic validation kept in the service for clear demo flow and backend safety.
        if (string.IsNullOrWhiteSpace(fullName) ||
            string.IsNullOrWhiteSpace(email) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(role))
        {
            return (false, "All fields are required.");
        }

        if (!EmployeeInputValidation.IsValidFullName(fullName))
        {
            return (false, "Full name must be between 2 and 120 characters.");
        }

        if (!EmployeeInputValidation.IsValidEmail(email))
        {
            return (false, "Please enter a valid email address.");
        }

        if (!EmployeeInputValidation.IsValidPassword(password))
        {
            return (false, "Password must be 8–10 characters and include a letter, a digit, and a special character.");
        }

        if (!EmployeeRoleNames.TryParse(role, out var parsedRole))
        {
            return (false, "Invalid role selected.");
        }

        if (actingEmployee is null)
        {
            if (parsedRole == EmployeeRole.Admin)
            {
                return (false, "Administrator accounts cannot be created from the employee self-registration screen.");
            }

            if (parsedRole is not (EmployeeRole.Secretary or EmployeeRole.Veterinarian))
            {
                return (false, "Self-registration is limited to secretary and veterinarian accounts.");
            }
        }
        else if (!RolePermissions.IsAdministrator(actingEmployee))
        {
            return (false, "Only an administrator can create this type of account.");
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var existingEmployee = await _employeeRepository.GetByEmailAsync(normalizedEmail);
        if (existingEmployee is not null)
        {
            return (false, "An employee with this email already exists.");
        }

        var normalizedUsername = (username ?? string.Empty).Trim();
        if (normalizedUsername.Length > 0)
        {
            var all = await _employeeRepository.GetAllAsync();
            if (all.Any(e =>
                    !string.IsNullOrWhiteSpace(e.Username) &&
                    e.Username.Equals(normalizedUsername, StringComparison.OrdinalIgnoreCase)))
            {
                return (false, "An employee with this username already exists.");
            }
        }

        var employee = new Employee
        {
            FullName = fullName.Trim(),
            Username = normalizedUsername,
            Email = normalizedEmail,
            Password = password.Trim(),
            Role = EmployeeRoleNames.ToStoredString(parsedRole)
        };

        await _employeeRepository.AddAsync(employee);
        return (true, "Employee registered successfully.");
    }
}
