namespace ClinicVets.Application.Security;

/// <summary>
/// Built-in bootstrap account for first-run desktop demos (stored alongside other employees in JSON).
/// </summary>
public static class SystemAccounts
{
    public const string DefaultAdminUsername = "admin";
    public const string DefaultAdminEmail = "admin@clinicvets.com";
    public const string DefaultAdminPassword = "Admin123!";
    public const string DefaultAdminDisplayName = "System Administrator";

    public static string DefaultAdminRole => EmployeeRoleNames.Admin;
}
