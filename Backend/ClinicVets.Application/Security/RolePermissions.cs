using ClinicVets.Core;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Security;

/// <summary>
/// Central RBAC rules for navigation and privileged operations.
/// </summary>
public static class RolePermissions
{
    public static bool IsAdministrator(Employee employee)
    {
        if (string.IsNullOrWhiteSpace(employee.Role))
            return false;
        return EmployeeRoleNames.TryParse(employee.Role, out var r) && r == EmployeeRole.Admin;
    }

    public static bool CanAccessDashboardSection(Employee employee, DashboardSection section)
    {
        if (!EmployeeRoleNames.TryParse(employee.Role, out var role))
            return section == DashboardSection.Home;

        return section switch
        {
            DashboardSection.Home => true,
            DashboardSection.Visits => role is EmployeeRole.Admin or EmployeeRole.Secretary or EmployeeRole.Veterinarian,
            DashboardSection.Patients => role is EmployeeRole.Admin or EmployeeRole.Veterinarian,
            DashboardSection.Billing => role is EmployeeRole.Admin or EmployeeRole.Secretary,
            DashboardSection.Staff => role == EmployeeRole.Admin,
            DashboardSection.PendingEmployees => role == EmployeeRole.Admin,
            _ => false
        };
    }
}

/// <summary>
/// High-level areas shown in the shell sidebar.
/// </summary>
public enum DashboardSection
{
    Home,
    Visits,
    Patients,
    Billing,
    Staff,
    PendingEmployees
}
