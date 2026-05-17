using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Shell;

public static class ShellNavPermissions
{
    public static bool CanAccess(Employee employee, ClinicShellNavKind nav)
    {
        var e = DemoModeSession.GetEffectiveEmployee(employee);
        return nav switch
        {
            ClinicShellNavKind.Dashboard => true,
            ClinicShellNavKind.Customers =>
                RolePermissions.CanAccessDashboardSection(e, DashboardSection.CustomerSearch) ||
                RolePermissions.CanAccessDashboardSection(e, DashboardSection.CustomerRegistration),
            ClinicShellNavKind.Animals =>
                RolePermissions.CanAccessDashboardSection(e, DashboardSection.CustomerAnimals),
            ClinicShellNavKind.Visits =>
                RolePermissions.CanAccessDashboardSection(e, DashboardSection.Visits),
            ClinicShellNavKind.Treatments =>
                RolePermissions.CanAccessDashboardSection(e, DashboardSection.Treatments),
            ClinicShellNavKind.UsersEmployees => RolePermissions.IsAdministrator(e),
            ClinicShellNavKind.PendingApprovals => RolePermissions.IsAdministrator(e),
            ClinicShellNavKind.Settings =>
                RolePermissions.CanAccessDashboardSection(e, DashboardSection.Settings),
            _ => false
        };
    }
}
