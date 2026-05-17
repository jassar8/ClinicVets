using ClinicVets.Core;

namespace ClinicVets.Application.Security;

/// <summary>
/// Canonical role strings persisted to JSON and shown in the UI.
/// </summary>
public static class EmployeeRoleNames
{
    public const string Admin = nameof(EmployeeRole.Admin);
    public const string Secretary = nameof(EmployeeRole.Secretary);
    public const string Veterinarian = nameof(EmployeeRole.Veterinarian);

    public static string ToStoredString(EmployeeRole role) => role.ToString();

    public static bool TryParse(string? text, out EmployeeRole role)
    {
        role = default;
        if (string.IsNullOrWhiteSpace(text))
            return false;

        var t = text.Trim();
        if (t.Equals("Administrator", StringComparison.OrdinalIgnoreCase))
        {
            role = EmployeeRole.Admin;
            return true;
        }

        return Enum.TryParse(t, ignoreCase: true, out role);
    }
}
