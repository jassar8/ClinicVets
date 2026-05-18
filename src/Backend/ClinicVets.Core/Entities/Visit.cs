namespace ClinicVets.Core.Entities;

/// <summary>Veterinary visit / treatment record (ported from main branch).</summary>
public sealed class Visit
{
    public int Id { get; set; }
    public string AnimalChipNumber { get; set; } = "";
    public DateTime VisitDate { get; set; }
    public string Reason { get; set; } = "";
    public string Symptoms { get; set; } = "";
    public string Diagnosis { get; set; } = "";
    public string VeterinarianName { get; set; } = "";
    public double BaseCost { get; set; }
    public string MedicationName { get; set; } = "";
    public int MedicationQuantity { get; set; }
    public double TotalCost { get; set; }
    public string ArrivalStatus { get; set; } = "Scheduled";
    public string ArrivalNote { get; set; } = "";
}
