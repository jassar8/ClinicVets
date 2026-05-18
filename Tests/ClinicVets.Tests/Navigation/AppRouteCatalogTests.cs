using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;
using ClinicVets.Desktop.Services;

namespace ClinicVets.Tests.Navigation;

public class AppRouteCatalogTests
{
    private static Employee Admin() => new() { Role = EmployeeRoleNames.Admin, Username = "admin", FullName = "Admin" };
    private static Employee Secretary() => new() { Role = EmployeeRoleNames.Secretary, Username = "sec", FullName = "Sec" };
    private static Employee Vet() => new() { Role = EmployeeRoleNames.Veterinarian, Username = "vet", FullName = "Vet" };

    [Theory]
    [InlineData("Admin", "Clients", true)]
    [InlineData("Admin", "Bills", true)]
    [InlineData("Admin", "Reports", true)]
    [InlineData("Admin", "Settings", true)]
    [InlineData("Vet", "Reports", false)]
    [InlineData("Secretary", "Bills", true)]
    [InlineData("Secretary", "Medications", false)]
    [InlineData("Vet", "Medications", true)]
    [InlineData("Vet", "Bills", false)]
    [InlineData("Vet", "Clients", false)]
    public void CanOpenRoute_MatchesRbac(string roleName, string route, bool expected)
    {
        var employee = roleName switch
        {
            "Admin" => Admin(),
            "Secretary" => Secretary(),
            _ => Vet()
        };

        Assert.Equal(expected, AppRouteCatalog.CanOpenRoute(employee, route));
    }

    [Fact]
    public void Bills_IsInImplementedRoutes()
    {
        Assert.Contains(AppRouteCatalog.Bills, AppRouteCatalog.ImplementedShellRoutes);
    }

    [Fact]
    public void ReportsAndSettings_AreImplementedInShell()
    {
        Assert.Contains(AppRouteCatalog.Reports, AppRouteCatalog.ImplementedShellRoutes);
        Assert.Contains(AppRouteCatalog.Settings, AppRouteCatalog.ImplementedShellRoutes);
        Assert.DoesNotContain(AppRouteCatalog.Reports, AppRouteCatalog.PlannedNotImplemented);
    }

    [Fact]
    public void Treatments_RemainsPlannedOnly()
    {
        Assert.Contains(AppRouteCatalog.Treatments, AppRouteCatalog.PlannedNotImplemented);
        Assert.DoesNotContain(AppRouteCatalog.Treatments, AppRouteCatalog.ImplementedShellRoutes);
    }
}
