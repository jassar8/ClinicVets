using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Infrastructure.Data;

namespace ClinicVets.Infrastructure.Demo;

/// <summary>Seeds in-memory customers, animals, employees, and the Demo Administrator for UI walkthroughs.</summary>
/// <remarks>No static fields that execute on type load — all work happens in <see cref="TryInitializeDemoData"/>.</remarks>
public static class DemoWorkspace
{
    /// <summary>Builds demo repositories and the Demo Admin in memory. Safe if no JSON or DB exists.</summary>
    public static bool TryInitializeDemoData(
        out IEmployeeRepository employees,
        out CustomerDirectoryService customers,
        out ICustomerDirectoryRepository customerDirectory,
        out Employee demoAdmin,
        out string errorMessage)
    {
        employees = null!;
        customers = null!;
        customerDirectory = null!;
        demoAdmin = null!;
        errorMessage = string.Empty;

        try
        {
            InitializeDemoData(out employees, out customers, out customerDirectory, out demoAdmin);
            return true;
        }
        catch (Exception ex)
        {
            errorMessage =
                "Demo Mode could not start. Details:" + Environment.NewLine + Environment.NewLine + ex.Message;
            return false;
        }
    }

    private static void InitializeDemoData(
        out IEmployeeRepository employees,
        out CustomerDirectoryService customers,
        out ICustomerDirectoryRepository customerDirectory,
        out Employee demoAdmin)
    {
        var demoAdminId = new Guid("00000000-0000-4000-8000-0000000000D1");

        demoAdmin = new Employee
        {
            Id = demoAdminId,
            FullName = "Demo Admin",
            Username = string.Empty,
            Email = "demo-admin@clinicvets.local",
            Password = string.Empty,
            Role = EmployeeRoleNames.Admin,
            RequestedRole = EmployeeRoleNames.Admin,
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "9001"
        };

        var seed = new List<Employee>
        {
            demoAdmin,
            new Employee
            {
                FullName = "Morgan Blake",
                Email = "morgan.blake@clinicvets.demo",
                Username = "mblake",
                Password = "Demo!",
                Role = EmployeeRoleNames.Secretary,
                RequestedRole = EmployeeRoleNames.Secretary,
                Status = EmployeeAccountStatusNames.Approved,
                EmployeeId = "3102"
            },
            new Employee
            {
                FullName = "Jamie Applicant",
                Email = "jamie.pending@clinicvets.demo",
                Username = string.Empty,
                Password = "Pending!",
                Role = string.Empty,
                RequestedRole = EmployeeRoleNames.Veterinarian,
                Status = EmployeeAccountStatusNames.Pending,
                EmployeeId = string.Empty
            },
            new Employee
            {
                FullName = "Taylor Register",
                Email = "taylor.pending@clinicvets.demo",
                Username = string.Empty,
                Password = "Pending!",
                Role = string.Empty,
                RequestedRole = EmployeeRoleNames.Secretary,
                Status = EmployeeAccountStatusNames.Pending,
                EmployeeId = string.Empty
            },
            new Employee
            {
                FullName = "Riley Request",
                Email = "riley.pending@clinicvets.demo",
                Username = string.Empty,
                Password = "Pending!",
                Role = string.Empty,
                RequestedRole = "Administrator",
                Status = EmployeeAccountStatusNames.Pending,
                EmployeeId = string.Empty
            }
        };

        employees = new InMemoryEmployeeRepository(seed);

        var c1 = new Customer
        {
            FullName = "Sarah Johnson",
            NationalId = "123456789",
            Phone = "+1-555-0101",
            Email = "sarah.johnson@example.com"
        };
        var c2 = new Customer
        {
            FullName = "Ahmed Hassan",
            NationalId = "234567890",
            Phone = "+1-555-0102",
            Email = "ahmed.hassan@example.com"
        };
        var c3 = new Customer
        {
            FullName = "Maria Garcia",
            NationalId = "345678901",
            Phone = "+1-555-0103",
            Email = "maria.garcia@example.com"
        };

        var animals = new List<Animal>
        {
            new() { CustomerId = c1.Id, Name = "Buddy", Species = "Golden Retriever" },
            new() { CustomerId = c1.Id, Name = "Luna", Species = "Siamese cat" },
            new() { CustomerId = c2.Id, Name = "Coco", Species = "Parakeet" },
            new() { CustomerId = c3.Id, Name = "Felix", Species = "Domestic shorthair" }
        };

        customerDirectory = new InMemoryCustomerDirectoryRepository([c1, c2, c3], animals);
        customers = new CustomerDirectoryService(customerDirectory);
    }
}
