namespace ClinicVets.Application.Validation;

/// <summary>
/// Input rules aligned with the page-log-in prototype (email + password strength).
/// </summary>
public static class EmployeeInputValidation
{
    public static bool IsValidFullName(string fullName)
    {
        var t = fullName.Trim();
        return t.Length is >= 2 and <= 120;
    }

    public static bool IsValidEmail(string email)
    {
        var t = email.Trim();
        return t.Contains('@') && t.Contains('.');
    }

    public static bool IsValidPassword(string password)
    {
        var p = password.Trim();
        if (p.Length < 8 || p.Length > 10)
        {
            return false;
        }

        var hasLetter = p.Any(char.IsLetter);
        var hasDigit = p.Any(char.IsDigit);
        var hasSpecial = p.Any(ch => !char.IsLetterOrDigit(ch));
        return hasLetter && hasDigit && hasSpecial;
    }
}
