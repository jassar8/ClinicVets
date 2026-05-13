using System.Text.RegularExpressions;

namespace ClinicVets.Application.Validation;

/// <summary>Validation rules for customer registration (secretary / admin workflows).</summary>
public static class CustomerInputValidation
{
    /// <summary>Latin letters, spaces, apostrophe, hyphen only (no digits).</summary>
    private static readonly Regex LettersOnlyName =
        new(@"^[A-Za-z\s'-]+$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex NineDigitId = new(@"^\d{9}$", RegexOptions.Compiled);

    private static readonly Regex EmailPattern = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static bool IsValidCustomerFullName(string? fullName)
    {
        var t = fullName?.Trim() ?? string.Empty;
        if (t.Length is < 2 or > 120)
            return false;
        return LettersOnlyName.IsMatch(t);
    }

    public static bool IsValidNationalId(string? nationalId)
    {
        var t = nationalId?.Trim() ?? string.Empty;
        return NineDigitId.IsMatch(t);
    }

    public static bool IsValidCustomerPhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
            return false;
        var digitCount = phone.Count(char.IsDigit);
        return digitCount is >= 9 and <= 15;
    }

    public static bool IsValidCustomerEmail(string? email)
    {
        var t = email?.Trim() ?? string.Empty;
        if (t.Length is < 5 or > 254)
            return false;
        return EmailPattern.IsMatch(t);
    }
}
