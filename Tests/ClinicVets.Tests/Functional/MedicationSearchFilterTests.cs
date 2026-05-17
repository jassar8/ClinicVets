using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;

namespace ClinicVets.Tests.Functional;

public sealed class MedicationSearchFilterTests
{
    private static Medication Sample(string name, int stock, DateTime expiration) =>
        new()
        {
            Id = 1,
            Name = name,
            StockQuantity = stock,
            UnitPrice = 10,
            ExpirationDate = expiration
        };

    [Fact]
    public void Matches_AllFilter_WhenSearchEmpty()
    {
        var med = Sample("Amoxicillin", 20, DateTime.Today.AddMonths(3));
        Assert.True(MedicationSearchFilter.Matches(med, null, MedicationSearchFilter.FilterAll));
    }

    [Fact]
    public void Matches_LowStockFilter_OnlyWhenStockLow()
    {
        var low = Sample("A", 3, DateTime.Today.AddMonths(3));
        var ok = Sample("B", 20, DateTime.Today.AddMonths(3));
        Assert.True(MedicationSearchFilter.Matches(low, null, MedicationSearchFilter.FilterLowStock));
        Assert.False(MedicationSearchFilter.Matches(ok, null, MedicationSearchFilter.FilterLowStock));
    }

    [Fact]
    public void Matches_ExpiringSoonFilter_WhenWithin30Days()
    {
        var soon = Sample("A", 20, DateTime.Today.AddDays(10));
        var later = Sample("B", 20, DateTime.Today.AddMonths(6));
        Assert.True(MedicationSearchFilter.Matches(soon, null, MedicationSearchFilter.FilterExpiringSoon));
        Assert.False(MedicationSearchFilter.Matches(later, null, MedicationSearchFilter.FilterExpiringSoon));
    }

    [Fact]
    public void Matches_SearchText_CaseInsensitive()
    {
        var med = Sample("Rimadyl", 10, DateTime.Today.AddMonths(2));
        Assert.True(MedicationSearchFilter.Matches(med, "rima", MedicationSearchFilter.FilterAll));
        Assert.False(MedicationSearchFilter.Matches(med, "aspirin", MedicationSearchFilter.FilterAll));
    }
}
