namespace ClinicVetsAvalonia.Helpers;

public static class UiIcons
{
    public const string Paw = "🐾";
    public const string Clients = "👥";
    public const string Animals = "🐾";
    public const string Visits = "🩺";
    public const string Medications = "💊";
    public const string Lock = "🔐";
    public const string Calendar = "📅";

    public const string ShowPassword = "הצג";
    public const string HidePassword = "הסתר";

    public static string GetAnimalIcon(string species) =>
        species switch
        {
            "כלב" or "Dog" => "🐶",
            "חתול" or "Cat" => "🐱",
            "זוחל" or "Reptile" => "🦎",
            "ציפור" or "Bird" => "🐦",
            _ => Paw
        };

    public static string GetGenderIcon(string gender) =>
        gender == "נקבה" ? "👩" : "👨";
}
