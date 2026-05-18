namespace ClinicVets.Core.Entities;

/// <summary>Animal registered to a customer.</summary>
public sealed class Animal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;

    /// <summary>7-digit chip (376xxxx) used by visits module (main branch compatibility).</summary>
    public string ChipNumber { get; set; } = string.Empty;

    public double Weight { get; set; } = 5.0;
    public DateTime BirthDate { get; set; } = DateTime.Today.AddYears(-2);
    public DateTime LastVaccinationDate { get; set; } = DateTime.Today.AddMonths(-6);

    /// <summary>Owner national ID — denormalized for UI ported from main.</summary>
    public string OwnerIdNumber { get; set; } = string.Empty;

    public override string ToString() => Name;
}
