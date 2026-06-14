using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Tests;

// Tests for visit cost calculation and migrating older single-medication visits to treatment lines.
public class VisitTreatmentTests
{
    // Total cost should equal the base cost plus the sum of all treatment line costs,
    // and the legacy medication fields should mirror the last line.
    [Fact]
    public void SyncLegacyMedicationFields_CalculatesTotalFromBaseAndLines()
    {
        var visit = new Visit
        {
            BaseCost = 100,
            TreatmentLines =
            {
                new VisitTreatmentLine { Description = "Exam", LineCost = 0 },
                new VisitTreatmentLine
                {
                    Description = "Antibiotic course",
                    MedicationName = "Amox",
                    MedicationQuantity = 2,
                    LineCost = 50
                }
            }
        };

        visit.SyncLegacyMedicationFields();

        Assert.Equal(150, visit.TotalCost);
        Assert.Equal("Amox", visit.MedicationName);
        Assert.Equal(2, visit.MedicationQuantity);
    }

    // An older visit stored without treatment lines can be migrated into a single line
    // without changing its existing total cost.
    [Fact]
    public void LegacyVisitWithoutLines_CanBeMigratedToSingleTreatmentLine()
    {
        var visit = new Visit
        {
            Id = 7,
            Diagnosis = "General checkup",
            MedicationName = "PainRelief",
            MedicationQuantity = 1,
            BaseCost = 80,
            TotalCost = 110
        };

        var medicationUnitPrice = 30.0;
        visit.TreatmentLines.Add(new VisitTreatmentLine
        {
            VisitId = visit.Id,
            Description = visit.Diagnosis,
            MedicationName = visit.MedicationName,
            MedicationQuantity = visit.MedicationQuantity,
            LineCost = medicationUnitPrice * visit.MedicationQuantity
        });

        visit.SyncLegacyMedicationFields();

        Assert.Single(visit.TreatmentLines);
        Assert.Equal("PainRelief", visit.MedicationName);
        Assert.Equal(110, visit.TotalCost);
    }
}
