namespace ClinicVets.Core.Entities;

/// <summary>Animal registered to a customer.</summary>
public sealed class Animal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CustomerId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Species { get; set; } = string.Empty;
}
