namespace ClinicVetsAvalonia.Models;

public class VisitTreatmentLine
{
    public int Id { get; set; }
    public int VisitId { get; set; }
    public string Description { get; set; } = "";
    public string MedicationName { get; set; } = "";
    public int MedicationQuantity { get; set; }
    public double LineCost { get; set; }
}
