using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;

namespace ClinicVets.Desktop.Forms;

internal static class ShellNavPermissions
{
    public static bool CanAccess(Employee employee, ClinicShellNavKind nav) =>
        nav switch
        {
            ClinicShellNavKind.Dashboard => true,
            ClinicShellNavKind.Customers =>
                RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerSearch) ||
                RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerRegistration),
            ClinicShellNavKind.Animals =>
                RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerAnimals),
            ClinicShellNavKind.Visits =>
                RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Visits),
            ClinicShellNavKind.Treatments =>
                RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Treatments),
            ClinicShellNavKind.UsersEmployees => RolePermissions.IsAdministrator(employee),
            ClinicShellNavKind.PendingApprovals => RolePermissions.IsAdministrator(employee),
            ClinicShellNavKind.Settings =>
                RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Settings),
            _ => false
        };
}
