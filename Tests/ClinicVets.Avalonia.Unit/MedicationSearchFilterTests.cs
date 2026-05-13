using System;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;
using Xunit;

namespace ClinicVets.Avalonia.Unit;

public class MedicationSearchFilterTests
{
    private static Medication M(string name, int stock, DateTime exp) =>
        new()
        {
            Name = name,
            StockQuantity = stock,
            UnitPrice = 1,
            ExpirationDate = exp,
            Notes = ""
        };

    [Fact]
    public void Empty_search_matches_all_filters_all()
    {
        var m = M("Aspirin", 10, DateTime.Today.AddMonths(6));
        Assert.True(MedicationSearchFilter.Matches(m, "", "הכל"));
    }

    [Fact]
    public void Search_is_case_insensitive_substring()
    {
        var m = M("IbuprofenX", 3, DateTime.Today.AddMonths(6));
        Assert.True(MedicationSearchFilter.Matches(m, "prof", "הכל"));
        Assert.False(MedicationSearchFilter.Matches(m, "zzz", "הכל"));
    }

    [Fact]
    public void Low_stock_filter()
    {
        var low = M("Low", 3, DateTime.Today.AddMonths(6));
        var high = M("High", 50, DateTime.Today.AddMonths(6));
        Assert.True(MedicationSearchFilter.Matches(low, "", "מלאי נמוך"));
        Assert.False(MedicationSearchFilter.Matches(high, "", "מלאי נמוך"));
    }

    [Fact]
    public void Expiring_soon_filter()
    {
        var soon = M("Soon", 10, DateTime.Today.AddDays(10));
        var far = M("Far", 10, DateTime.Today.AddMonths(12));
        Assert.True(MedicationSearchFilter.Matches(soon, "", "תוקף קרוב"));
        Assert.False(MedicationSearchFilter.Matches(far, "", "תוקף קרוב"));
    }
}
