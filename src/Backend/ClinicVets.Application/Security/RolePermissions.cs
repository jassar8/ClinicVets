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

        if (role == EmployeeRole.Admin)
            return true;

        return section switch
        {
            DashboardSection.Home => true,
            DashboardSection.Visits => role == EmployeeRole.Veterinarian,
            DashboardSection.Patients => role == EmployeeRole.Veterinarian,
            DashboardSection.Billing => role == EmployeeRole.Secretary,
            DashboardSection.Staff => false,
            DashboardSection.PendingEmployees => false,
            DashboardSection.CustomerRegistration => role == EmployeeRole.Secretary,
            DashboardSection.CustomerSearch => role == EmployeeRole.Secretary,
            DashboardSection.CustomerAnimals => role is EmployeeRole.Secretary or EmployeeRole.Veterinarian,
            DashboardSection.Treatments => role == EmployeeRole.Veterinarian,
            DashboardSection.Settings => role == EmployeeRole.Admin,
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
    PendingEmployees,
    CustomerRegistration,
    CustomerSearch,
    CustomerAnimals,
    Treatments,
    Settings
}
