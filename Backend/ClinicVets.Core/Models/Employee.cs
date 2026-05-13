namespace ClinicVets.Core.Entities;

/// <summary>
/// Clinic employee domain model (shared across UI and services).
/// </summary>
public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    /// <summary>Optional sign-in alias (e.g. default admin uses "admin").</summary>
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    /// <summary>Pending until an administrator approves; rejected requests stay rejected.</summary>
    public string Status { get; set; } = EmployeeAccountStatusNames.Pending;
    /// <summary>Four-digit clinic ID assigned only when an administrator approves the account.</summary>
    public string EmployeeId { get; set; } = string.Empty;
}
