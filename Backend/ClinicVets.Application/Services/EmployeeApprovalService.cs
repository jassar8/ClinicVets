using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Security;
using ClinicVets.Application.Validation;
using ClinicVets.Core;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Services;

public sealed class EmployeeApprovalService
{
    private readonly IEmployeeRepository _repository;

    public EmployeeApprovalService(IEmployeeRepository repository)
    {
        _repository = repository;
    }

    public Task<IReadOnlyList<Employee>> GetPendingAsync() => _repository.GetPendingRegistrationsAsync();

    public async Task<(bool Ok, string Message)> ApproveAsync(Guid employeeId, string finalRole, Employee actingAdmin)
    {
        if (!RolePermissions.IsAdministrator(actingAdmin))
            return (false, "Only an administrator can approve registrations.");

        if (!EmployeeRoleNames.TryParse(finalRole, out var finalParsed) ||
            finalParsed is not (EmployeeRole.Admin or EmployeeRole.Secretary or EmployeeRole.Veterinarian))
        {
            return (false, "Final role must be Secretary, Veterinarian, or Administrator.");
        }

        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee is null)
            return (false, "Employee not found.");

        if (!string.Equals(employee.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
            return (false, "Only pending registrations can be approved.");

        var all = await _repository.GetAllAsync();
        var assigned = EmployeeIdAllocation.TryAllocateNext(all);
        if (assigned is null)
            return (false, "No available employee IDs in the 1001–9999 range.");

        if (string.IsNullOrWhiteSpace(employee.RequestedRole))
            employee.RequestedRole = (employee.Role ?? string.Empty).Trim();

        employee.Role = EmployeeRoleNames.ToStoredString(finalParsed);
        employee.Status = EmployeeAccountStatusNames.Approved;
        employee.EmployeeId = assigned;
        await _repository.UpdateAsync(employee);
        return (true, $"Employee approved. Employee ID {assigned} was assigned.");
    }

    public async Task<(bool Ok, string Message)> RejectAsync(Guid employeeId, Employee actingAdmin)
    {
        if (!RolePermissions.IsAdministrator(actingAdmin))
            return (false, "Only an administrator can reject registrations.");

        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee is null)
            return (false, "Employee not found.");

        if (!string.Equals(employee.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
            return (false, "Only pending registrations can be rejected.");

        employee.Status = EmployeeAccountStatusNames.Rejected;
        employee.EmployeeId = string.Empty;
        await _repository.UpdateAsync(employee);
        return (true, "Registration rejected.");
    }
}
