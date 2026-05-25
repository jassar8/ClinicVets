using System;
using System.Collections.Generic;
using System.Linq;

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
        public string ArrivalStatus { get; set; } = "Scheduled";
        public string ArrivalNote { get; set; } = "";
        public List<VisitTreatmentLine> TreatmentLines { get; set; } = new();

        public void SyncLegacyMedicationFields()
        {
            var firstWithMedication = TreatmentLines
                .FirstOrDefault(line => !string.IsNullOrWhiteSpace(line.MedicationName));

            MedicationName = firstWithMedication?.MedicationName ?? "";
            MedicationQuantity = firstWithMedication?.MedicationQuantity ?? 0;
            TotalCost = BaseCost + TreatmentLines.Sum(line => line.LineCost);
        }
    }
}
