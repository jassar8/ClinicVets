using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;

namespace ClinicVets.Desktop;

/// <summary>Canonical route names for navigation tests and stability walkthroughs.</summary>
public static class AppRouteCatalog
{
    public const string Startup = "Startup";
    public const string Login = "Login";
    public const string RegisterEmployee = "RegisterEmployee";
    public const string ForgotPassword = "ForgotPassword";
    public const string MainMenu = "MainMenu";
    public const string Clients = "Clients";
    public const string Animals = "Animals";
    public const string Visits = "Visits";
    public const string Medications = "Medications";
    public const string Bills = "Bills";
    public const string Reports = "Reports";
    public const string Settings = "Settings";
    public const string Treatments = "Treatments";
    public const string Logout = "Logout";

    public static IReadOnlyList<string> AllRoutes { get; } =
    [
        Startup, Login, RegisterEmployee, ForgotPassword, MainMenu,
        Clients, Animals, Visits, Medications, Bills,
        Reports, Settings, Treatments, Logout
    ];

    public static IReadOnlyList<string> ImplementedShellRoutes { get; } =
    [
        Login, RegisterEmployee, ForgotPassword, MainMenu,
        Clients, Animals, Visits, Medications, Bills, Logout
    ];

    public static IReadOnlyList<string> PlannedNotImplemented { get; } =
    [
        Reports, Settings, Treatments
    ];

    public static bool CanOpenRoute(Employee employee, string route) => route switch
    {
        Login or RegisterEmployee or ForgotPassword or MainMenu or Logout => true,
        Clients => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerSearch) ||
                   RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerRegistration),
        Animals => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.CustomerAnimals),
        Visits => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Visits),
        Medications => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Treatments),
        Bills => RolePermissions.CanAccessDashboardSection(employee, DashboardSection.Billing),
        Reports or Settings or Treatments => false,
        _ => false
    };
}
