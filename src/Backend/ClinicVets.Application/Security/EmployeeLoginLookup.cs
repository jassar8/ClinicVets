using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Security;

/// <summary>
/// Resolves an employee by email or username, including bootstrap admin alias rules.
/// </summary>
public static class EmployeeLoginLookup
{
    /// <param name="loginIdentifier">Expected to be trimmed by the caller.</param>
    public static Employee? FindEmployee(IEnumerable<Employee> employees, string loginIdentifier)
    {
        if (string.IsNullOrEmpty(loginIdentifier))
            return null;

        if (loginIdentifier.Contains('@', StringComparison.Ordinal))
        {
            return employees.FirstOrDefault(e =>
                string.Equals(e.Email?.Trim(), loginIdentifier, StringComparison.OrdinalIgnoreCase));
        }

        var match = employees.FirstOrDefault(e =>
            (!string.IsNullOrWhiteSpace(e.Username) &&
             string.Equals(e.Username.Trim(), loginIdentifier, StringComparison.OrdinalIgnoreCase)) ||
            string.Equals(e.Email?.Trim(), loginIdentifier, StringComparison.OrdinalIgnoreCase));

        if (match is not null)
            return match;

        // Legacy JSON often omitted Username; still allow the documented default alias.
        if (string.Equals(loginIdentifier, SystemAccounts.DefaultAdminUsername, StringComparison.OrdinalIgnoreCase))
        {
            return employees.FirstOrDefault(e =>
                string.Equals(e.Email?.Trim(), SystemAccounts.DefaultAdminEmail, StringComparison.OrdinalIgnoreCase));
        }

        return null;
    }
}
