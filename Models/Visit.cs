using System;

namespace ClinicVetsAvalonia.Models
{
    public class Visit
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
    }
}
