using System;
using System.IO;
using ClinicVetsAvalonia.Data;

namespace ClinicVetsAvalonia.Tests;

public class FakeDataSeederTests
{
    [Fact]
    public void SeedData_PassesAllValidationRules()
    {
        Assert.True(FakeDataSeeder.ValidateSeedData());
    }

    [Fact]
    public void SeedAllIfEmpty_PopulatesDatabaseOnce()
    {
        string tempDb = Path.Combine(Path.GetTempPath(), $"clinicvets-seed-{Guid.NewGuid():N}.db");
        string? previousDb = Environment.GetEnvironmentVariable("CLINICVETS_DB");

        try
        {
            Environment.SetEnvironmentVariable("CLINICVETS_DB", tempDb);
            DatabaseInitializer.Initialize();
            AppData.ReloadAll();

            Assert.Equal(6, AppData.Employees.Count);
            Assert.Equal(5, AppData.Clients.Count);
            Assert.True(AppData.Animals.Count >= 5);
            Assert.True(AppData.Medications.Count >= 5);
            Assert.True(AppData.Visits.Count >= 5);

            int employeeCountBefore = AppData.Employees.Count;
            DatabaseInitializer.Initialize();
            AppData.ReloadAll();

            Assert.Equal(employeeCountBefore, AppData.Employees.Count);
            Assert.Equal(5, AppData.Clients.Count);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CLINICVETS_DB", previousDb);
            try
            {
                if (File.Exists(tempDb))
                    File.Delete(tempDb);
            }
            catch (IOException)
            {
            }
        }
    }
}
