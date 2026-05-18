using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core.Entities;
using ClinicVets.Infrastructure.Repositories;
using ClinicVets.Infrastructure.Demo;

namespace ClinicVets.Desktop.Services;

/// <summary>Wires v2 JSON-backed services for the Avalonia UI (initialized once at startup).</summary>
public static class AppServices
{
    public static IEmployeeRepository Employees { get; private set; } = null!;
    public static ICustomerDirectoryRepository CustomerStore { get; private set; } = null!;
    public static IMedicationRepository MedicationStore { get; private set; } = null!;
    public static IVisitRepository VisitStore { get; private set; } = null!;

    public static EmployeeAuthenticationService Auth { get; private set; } = null!;
    public static EmployeeRegistrationService Registration { get; private set; } = null!;
    public static EmployeeApprovalService Approvals { get; private set; } = null!;
    public static CustomerDirectoryService Customers { get; private set; } = null!;
    public static MedicationInventoryService Medications { get; private set; } = null!;
    public static VisitManagementService Visits { get; private set; } = null!;
    public static EmployeePasswordResetService PasswordReset { get; private set; } = null!;

    public static bool IsDemoMode { get; private set; }

    public static void Initialize()
    {
        IsDemoMode = false;
        Employees = new JsonFileEmployeeRepository();
        CustomerStore = new JsonFileCustomerDirectoryRepository();
        MedicationStore = new JsonFileMedicationRepository();
        VisitStore = new JsonFileVisitRepository();

        Auth = new EmployeeAuthenticationService(Employees);
        Registration = new EmployeeRegistrationService(Employees);
        Approvals = new EmployeeApprovalService(Employees);
        Customers = new CustomerDirectoryService(CustomerStore);
        Medications = new MedicationInventoryService(MedicationStore);
        Visits = new VisitManagementService(VisitStore);
        PasswordReset = new EmployeePasswordResetService(Employees);
    }

    /// <summary>In-memory demo workspace (same flow as legacy WinForms <c>NavigateToDemo</c>).</summary>
    public static bool TryEnterDemoMode(out Employee demoAdmin, out string errorMessage)
    {
        demoAdmin = null!;
        errorMessage = string.Empty;

        if (!DesktopBuildOptions.EnableDemoMode)
        {
            errorMessage = "Demo Mode is disabled for this build.";
            return false;
        }

        if (!DemoWorkspace.TryInitializeDemoData(
                out var employees,
                out var customers,
                out var customerDirectory,
                out demoAdmin,
                out errorMessage))
            return false;

        Employees = employees;
        CustomerStore = customerDirectory;
        Customers = customers;
        MedicationStore = new InMemoryMedicationRepository(DemoWorkspace.CreateDemoMedications());
        VisitStore = new InMemoryVisitRepository([]);
        Auth = new EmployeeAuthenticationService(Employees);
        Registration = new EmployeeRegistrationService(Employees);
        Approvals = new EmployeeApprovalService(Employees);
        Medications = new MedicationInventoryService(MedicationStore);
        Visits = new VisitManagementService(VisitStore);
        PasswordReset = new EmployeePasswordResetService(Employees);
        IsDemoMode = true;
        return true;
    }

    public static void ExitDemoMode()
    {
        if (!IsDemoMode)
            return;

        IsDemoMode = false;
        Initialize();
    }
}

