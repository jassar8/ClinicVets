namespace ClinicVets.Application.Validation;

/// <summary>Rules for four-digit employee numbers (manual admin-created accounts and auto-allocated approval IDs).</summary>
public static class EmployeeIdValidation
{
    public static bool IsFourDigitEmployeeId(string? value)
    {
        var t = value?.Trim() ?? string.Empty;
        return t.Length == 4 && t.All(char.IsDigit);
    }
}
