using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Validation;

/// <summary>Assigns sequential four-digit employee numbers for self-service approvals.</summary>
public static class EmployeeIdAllocation
{
    public const int MinimumAutoId = 1001;
    public const int MaximumAutoId = 9999;

    /// <summary>
    /// Returns the smallest unused ID in <see cref="MinimumAutoId"/>–<see cref="MaximumAutoId"/>
    /// (inclusive) based on current <see cref="Employee.EmployeeId"/> values, or null if the range is full.
    /// </summary>
    public static string? TryAllocateNext(IEnumerable<Employee> employees)
    {
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var e in employees)
        {
            var id = e.EmployeeId?.Trim() ?? string.Empty;
            if (EmployeeIdValidation.IsFourDigitEmployeeId(id))
                used.Add(id);
        }

        for (var n = MinimumAutoId; n <= MaximumAutoId; n++)
        {
            var candidate = n.ToString("D4");
            if (!used.Contains(candidate))
                return candidate;
        }

        return null;
    }
}
