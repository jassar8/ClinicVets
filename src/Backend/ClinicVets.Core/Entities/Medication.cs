namespace ClinicVets.Core.Entities;

/// <summary>Clinic medicine stock item (inventory).</summary>
public sealed class Medication
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int StockQuantity { get; set; }
    public double UnitPrice { get; set; }
    public DateTime ExpirationDate { get; set; } = DateTime.Today.AddMonths(6);
    public string Notes { get; set; } = "";

    public bool IsLowStock => StockQuantity <= 5;
    public bool IsExpiringSoon => ExpirationDate.Date <= DateTime.Today.AddDays(30);

    public override string ToString() => Name;
}
