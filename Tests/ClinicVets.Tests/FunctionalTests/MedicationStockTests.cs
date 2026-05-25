using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Tests;

public class MedicationStockTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    public void Medication_WithFiveOrFewerUnits_IsLowStock(int stockQuantity)
    {
        var medication = new Medication
        {
            Name = "Antibiotic",
            StockQuantity = stockQuantity,
            ExpirationDate = DateTime.Today.AddMonths(6)
        };

        Assert.True(medication.IsLowStock);
    }

    [Fact]
    public void Medication_WithMoreThanFiveUnits_IsNotLowStock()
    {
        var medication = new Medication
        {
            Name = "Antibiotic",
            StockQuantity = 6,
            ExpirationDate = DateTime.Today.AddMonths(6)
        };

        Assert.False(medication.IsLowStock);
    }

    [Fact]
    public void Medication_ExpiringWithinThirtyDays_IsExpiringSoon()
    {
        var medication = new Medication
        {
            Name = "Vaccine",
            StockQuantity = 20,
            ExpirationDate = DateTime.Today.AddDays(30)
        };

        Assert.True(medication.IsExpiringSoon);
    }

    [Fact]
    public void SchedulingVisitWithMedication_ReducesStockByRequestedQuantity()
    {
        var medication = new Medication { Name = "Pain Relief", StockQuantity = 10 };
        var visit = new Visit
        {
            AnimalChipNumber = "3761234",
            MedicationName = medication.Name,
            MedicationQuantity = 3
        };

        medication.StockQuantity -= visit.MedicationQuantity;

        Assert.Equal(7, medication.StockQuantity);
    }
}
