using System;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Services;

/// <summary>
/// Search/filter rules for the medicine inventory (shared by UI and unit tests).
/// </summary>
public static class MedicationSearchFilter
{
    public static bool Matches(Medication medication, string? searchTextTrimmed, string filterLabel)
    {
        bool matchesSearch = string.IsNullOrWhiteSpace(searchTextTrimmed) ||
                             medication.Name.Contains(searchTextTrimmed!, StringComparison.OrdinalIgnoreCase);

        bool matchesFilter = filterLabel switch
        {
            "מלאי נמוך" => medication.IsLowStock,
            "תוקף קרוב" => medication.IsExpiringSoon,
            _ => true
        };

        return matchesSearch && matchesFilter;
    }
}
