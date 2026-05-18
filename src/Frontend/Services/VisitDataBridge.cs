using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;

namespace ClinicVets.Desktop.Services;

/// <summary>
/// Compatibility layer for the visits UI ported from main branch (replaces ClinicVetsAvalonia.Data.AppData).
/// </summary>
public static class VisitDataBridge
{
    public static List<Customer> Clients { get; } = [];
    public static List<Animal> Animals { get; } = [];
    public static List<Medication> Medications { get; } = [];
    public static List<Visit> Visits { get; } = [];

    public static async Task RefreshAsync()
    {
        Clients.Clear();
        Animals.Clear();
        Medications.Clear();
        Visits.Clear();

        Clients.AddRange(await AppServices.Customers.ListCustomersAsync());
        Animals.AddRange(await AppServices.CustomerStore.GetAllAnimalsAsync());

        var meds = await AppServices.Medications.SearchAsync(null, MedicationSearchFilter.FilterAll);
        Medications.AddRange(meds);

        var visits = await AppServices.Visits.GetAllAsync();
        Visits.AddRange(visits);
    }

    public static void SaveVisitsToDatabase() =>
        AppServices.Visits.SaveAllAsync(Visits.ToList()).GetAwaiter().GetResult();

    public static async Task SaveVisitsToDatabaseAsync() =>
        await AppServices.Visits.SaveAllAsync(Visits.ToList());

    public static void SaveMedicationsToDatabase()
    {
        _ = SaveMedicationsToDatabaseAsync();
    }

    public static async Task SaveMedicationsToDatabaseAsync()
    {
        foreach (var medication in Medications)
        {
            await AppServices.Medications.UpdateAsync(
                medication.Id,
                medication.StockQuantity,
                medication.UnitPrice,
                medication.ExpirationDate,
                medication.Notes);
        }

        await RefreshAsync();
    }
}
