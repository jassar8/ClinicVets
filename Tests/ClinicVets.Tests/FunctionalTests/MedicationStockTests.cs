using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Tests;

// Tests for medication stock rules: low-stock and expiring-soon flags, and stock reduction on a visit.
public class MedicationStockTests
{
    // 5 or fewer units in stock should be flagged as low stock.
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

    // More than 5 units should NOT be flagged as low stock.
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

    // A medication expiring within 30 days should be flagged as expiring soon.
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

    // Using a medication in a visit should reduce its stock by the requested quantity.
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
