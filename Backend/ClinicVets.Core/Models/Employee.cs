namespace ClinicVets.Core.Entities;

/// <summary>
/// Clinic employee domain model (shared across UI and services).
/// </summary>
public class Employee
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}
