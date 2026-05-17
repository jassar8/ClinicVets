namespace ClinicVets.Application.Validation;

public static class MedicationInputValidation
{
    public static bool IsRequiredName(string name) =>
        !string.IsNullOrWhiteSpace(name) && name.Trim().Length <= 120;

    public static bool IsValidStockQuantity(int quantity) => quantity >= 0;

    public static bool IsValidUnitPrice(double price) => price >= 0 && !double.IsNaN(price) && !double.IsInfinity(price);
}
