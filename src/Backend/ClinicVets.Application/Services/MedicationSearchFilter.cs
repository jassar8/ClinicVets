using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

/// <summary>Search and stock-alert rules for medicine inventory (shared by UI and tests).</summary>
public static class MedicationSearchFilter
{
    public const string FilterAll = "All";
    public const string FilterLowStock = "Low stock";
    public const string FilterExpiringSoon = "Expiring soon";

    public static bool Matches(Medication medication, string? searchTextTrimmed, string filterLabel)
    {
        var matchesSearch = string.IsNullOrWhiteSpace(searchTextTrimmed) ||
                            medication.Name.Contains(searchTextTrimmed!, StringComparison.OrdinalIgnoreCase);

        var matchesFilter = filterLabel switch
        {
            FilterLowStock => medication.IsLowStock,
            FilterExpiringSoon => medication.IsExpiringSoon,
            _ => true
        };

        return matchesSearch && matchesFilter;
    }
}
