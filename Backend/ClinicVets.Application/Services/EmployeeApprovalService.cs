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

    public async Task<(bool Ok, string Message)> ApproveAsync(
        Guid employeeId,
        string fourDigitId,
        string finalRole,
        Employee actingAdmin)
    {
        if (!RolePermissions.IsAdministrator(actingAdmin))
            return (false, "Only an administrator can approve registrations.");

        if (!EmployeeIdValidation.IsFourDigitEmployeeId(fourDigitId))
            return (false, "Employee ID must be exactly four digits.");

        if (!EmployeeRoleNames.TryParse(finalRole, out var finalParsed) ||
            finalParsed is not (EmployeeRole.Admin or EmployeeRole.Secretary or EmployeeRole.Veterinarian))
        {
            return (false, "Final role must be Secretary, Veterinarian, or Administrator.");
        }

        var trimmedId = fourDigitId.Trim();
        var all = await _repository.GetAllAsync();
        if (all.Any(e =>
                e.Id != employeeId &&
                EmployeeIdValidation.IsFourDigitEmployeeId(e.EmployeeId) &&
                string.Equals(e.EmployeeId.Trim(), trimmedId, StringComparison.Ordinal)))
            return (false, "That Employee ID is already in use.");

        var employee = await _repository.GetByIdAsync(employeeId);
        if (employee is null)
            return (false, "Employee not found.");

        if (!string.Equals(employee.Status?.Trim(), EmployeeAccountStatusNames.Pending, StringComparison.OrdinalIgnoreCase))
            return (false, "Only pending registrations can be approved.");

        if (string.IsNullOrWhiteSpace(employee.RequestedRole))
            employee.RequestedRole = (employee.Role ?? string.Empty).Trim();

        employee.Role = EmployeeRoleNames.ToStoredString(finalParsed);
        employee.Status = EmployeeAccountStatusNames.Approved;
        employee.EmployeeId = trimmedId;
        await _repository.UpdateAsync(employee);
        return (true, "Employee approved.");
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
