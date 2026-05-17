namespace ClinicVets.Core.Entities;

/// <summary>Clinic customer (pet owner) stored locally for the desktop demo.</summary>
public sealed class Customer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    /// <summary>National ID — exactly nine digits (Israeli teudat zehut style).</summary>
    public string NationalId { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public override string ToString() => $"{FullName} ({NationalId})";
}
