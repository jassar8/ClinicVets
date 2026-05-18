namespace ClinicVets.Desktop.Helpers;

/// <summary>Hebrew inline validation messages for forms (UI only; persistence uses v2 services).</summary>
public static class UiFormValidation
{
    public static bool IsValidEmail(string email) =>
        !string.IsNullOrWhiteSpace(email) && email.Contains('@') && email.Contains('.');

    public static bool IsValidPassword(string password)
    {
        var p = password.Trim();
        if (p.Length < 8 || p.Length > 10)
            return false;
        return p.Any(char.IsLetter) && p.Any(char.IsDigit) && p.Any(ch => !char.IsLetterOrDigit(ch));
    }

    public static bool IsValidNationalId(string id) =>
        id.Length == 9 && id.All(char.IsDigit);

    public static bool IsValidStockQuantity(int quantity) => quantity >= 0;

    public static bool IsValidMoney(double price) => price >= 0 && !double.IsNaN(price);

    public static bool IsRequiredText(string text) => !string.IsNullOrWhiteSpace(text);

    public static bool IsValidAnimalName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.All(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch));

    public static bool IsValidChipNumber(string chipNumber) =>
        !string.IsNullOrWhiteSpace(chipNumber) &&
        chipNumber.Length == 7 &&
        chipNumber.StartsWith("376", StringComparison.Ordinal) &&
        chipNumber.All(char.IsDigit);

    public static bool IsValidVisitDate(DateTime visitDate) => visitDate.Year >= 2000;

    public static bool IsValidMedicationQuantity(int quantity) => quantity > 0;

    public static bool IsVaccinationDue(DateTime lastVaccinationDate) =>
        lastVaccinationDate.Date <= DateTime.Today.AddMonths(-12);
}
