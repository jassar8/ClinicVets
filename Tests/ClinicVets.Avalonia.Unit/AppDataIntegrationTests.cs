using System;
using System.IO;
using System.Linq;
using ClinicVetsAvalonia.Database;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Repositories;
using Xunit;

namespace ClinicVets.Avalonia.Unit;

/// <summary>
/// White-box integration against real SQLite in a temp folder (mirrors login/medicine persistence logic).
/// </summary>
public class AppDataIntegrationTests : IDisposable
{
    private readonly string _dir;

    public AppDataIntegrationTests()
    {
        _dir = Path.Combine(Path.GetTempPath(), "ClinicVetsTest_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_dir);
        DbPaths.SetDatabaseFolderOverrideForTests(_dir);
        AppData.Initialize();
    }

    public void Dispose()
    {
        DbPaths.SetDatabaseFolderOverrideForTests(null);
        try
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }
        catch
        {
            // best-effort cleanup on locked files
        }
    }

    [Fact]
    public void Default_seed_creates_admin_and_vet_with_expected_passwords()
    {
        var admin = AppData.Employees.FirstOrDefault(e => e.Username == "admin");
        var vet = AppData.Employees.FirstOrDefault(e => e.Username == "vet");
        Assert.NotNull(admin);
        Assert.NotNull(vet);
        Assert.Equal("1234", admin!.Password);
        Assert.Equal("1234", vet!.Password);
        Assert.Equal("Secretary", admin.Role);
        Assert.Equal("Vet", vet.Role);
    }

    [Fact]
    public void Login_predicate_matches_valid_credentials()
    {
        var ok = AppData.Employees.FirstOrDefault(e =>
            e.Username == "admin" && e.Password == "1234");
        Assert.NotNull(ok);
    }

    [Fact]
    public void Login_predicate_rejects_wrong_password()
    {
        var bad = AppData.Employees.FirstOrDefault(e =>
            e.Username == "admin" && e.Password == "wrong");
        Assert.Null(bad);
    }

    [Fact]
    public void Duplicate_username_detection_matches_register_view_rules()
    {
        Assert.Contains(AppData.Employees, e => e.Username == "admin");
        bool exists = AppData.Employees.Any(e => e.Username == "admin");
        Assert.True(exists);
    }

    [Fact]
    public void Medication_add_save_reload_roundtrip()
    {
        var med = new Medication
        {
            Name = "TestMed_" + Guid.NewGuid().ToString("N")[..8],
            StockQuantity = 10,
            UnitPrice = 5.5,
            ExpirationDate = DateTime.Today.AddMonths(6),
            Notes = "t"
        };

        AppData.Medications.Add(med);
        AppData.SaveMedicationsToDatabase();

        AppData.Medications.Clear();
        AppData.LoadMedications();

        Assert.Contains(AppData.Medications, m => m.Name == med.Name && m.StockQuantity == 10);
    }

    [Fact]
    public void Medication_update_persists()
    {
        var name = "UpdMed_" + Guid.NewGuid().ToString("N")[..8];
        var med = new Medication
        {
            Name = name,
            StockQuantity = 5,
            UnitPrice = 1,
            ExpirationDate = DateTime.Today.AddMonths(3),
            Notes = ""
        };
        AppData.Medications.Add(med);
        AppData.SaveMedicationsToDatabase();

        var loaded = AppData.Medications.First(m => m.Name == name);
        loaded.StockQuantity = 99;
        loaded.UnitPrice = 12.25;
        AppData.SaveMedicationsToDatabase();

        AppData.Medications.Clear();
        AppData.LoadMedications();
        var again = AppData.Medications.First(m => m.Name == name);
        Assert.Equal(99, again.StockQuantity);
        Assert.Equal(12.25, again.UnitPrice);
    }

    [Fact]
    public void Medication_delete_persists()
    {
        var name = "DelMed_" + Guid.NewGuid().ToString("N")[..8];
        AppData.Medications.Add(new Medication
        {
            Name = name,
            StockQuantity = 1,
            UnitPrice = 1,
            ExpirationDate = DateTime.Today.AddMonths(1),
            Notes = ""
        });
        AppData.SaveMedicationsToDatabase();

        var m = AppData.Medications.First(x => x.Name == name);
        AppData.Medications.Remove(m);
        AppData.SaveMedicationsToDatabase();

        AppData.Medications.Clear();
        AppData.LoadMedications();
        Assert.DoesNotContain(AppData.Medications, x => x.Name == name);
    }
}
