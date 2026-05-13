using ClinicVets.Application.Security;
using ClinicVets.Core;
using ClinicVets.Core.Entities;

namespace ClinicVets.Tests.Functional;

public class RolePermissionsCustomerSectionsTests
{
    private static Employee Emp(EmployeeRole role) => new()
    {
        FullName = "U",
        Email = "u@x.com",
        Password = "x",
        Role = role.ToString(),
        Status = EmployeeAccountStatusNames.Approved,
        EmployeeId = "1001"
    };

    [Theory]
    [InlineData(EmployeeRole.Admin, DashboardSection.CustomerRegistration, true)]
    [InlineData(EmployeeRole.Secretary, DashboardSection.CustomerSearch, true)]
    [InlineData(EmployeeRole.Secretary, DashboardSection.CustomerAnimals, true)]
    [InlineData(EmployeeRole.Veterinarian, DashboardSection.CustomerRegistration, false)]
    [InlineData(EmployeeRole.Veterinarian, DashboardSection.CustomerSearch, false)]
    [InlineData(EmployeeRole.Veterinarian, DashboardSection.CustomerAnimals, true)]
    [InlineData(EmployeeRole.Veterinarian, DashboardSection.Visits, true)]
    [InlineData(EmployeeRole.Veterinarian, DashboardSection.Treatments, true)]
    [InlineData(EmployeeRole.Veterinarian, DashboardSection.Settings, false)]
    [InlineData(EmployeeRole.Secretary, DashboardSection.Visits, false)]
    [InlineData(EmployeeRole.Secretary, DashboardSection.Treatments, false)]
    [InlineData(EmployeeRole.Secretary, DashboardSection.Settings, false)]
    public void Dashboard_sections_follow_role_rules(EmployeeRole role, DashboardSection section, bool allowed)
    {
        var e = Emp(role);
        Assert.Equal(allowed, RolePermissions.CanAccessDashboardSection(e, section));
    }
}
