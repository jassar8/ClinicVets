using System.Collections.Concurrent;
using ClinicVets.Application.Interfaces;
using ClinicVets.Application.Validation;

namespace ClinicVets.Application.Services;

public sealed class EmployeePasswordResetService
{
    private static readonly ConcurrentDictionary<string, PendingReset> Pending =
        new(StringComparer.OrdinalIgnoreCase);

    private readonly IEmployeeRepository _employees;
    private readonly TimeSpan _codeLifetime = TimeSpan.FromMinutes(10);

    public EmployeePasswordResetService(IEmployeeRepository employees)
    {
        _employees = employees;
    }

    public async Task<(bool IsSuccess, string Message)> RequestCodeAsync(string email)
    {
        var normalized = email.Trim();
        if (!EmployeeInputValidation.IsValidEmail(normalized))
            return (false, "Enter a valid clinic email address.");

        var employee = await _employees.GetByEmailAsync(normalized);
        if (employee is null)
            return (false, "No employee account is registered with that email.");

        var code = PasswordResetDelivery.GenerateCode();
        Pending[normalized] = new PendingReset(code, DateTime.UtcNow.Add(_codeLifetime));
        var deliveryMessage = await PasswordResetDelivery.SendResetCodeAsync(normalized, code);
        return (true, deliveryMessage);
    }

    public async Task<(bool IsSuccess, string Message)> ResetPasswordAsync(string email, string code, string newPassword)
    {
        var normalized = email.Trim();
        if (!EmployeeInputValidation.IsValidEmail(normalized))
            return (false, "Enter a valid clinic email address.");

        if (string.IsNullOrWhiteSpace(code))
            return (false, "Enter the verification code.");

        if (!EmployeeInputValidation.IsValidPassword(newPassword))
            return (false, "Password must be 8–10 characters and include letters, digits, and a special character.");

        if (!Pending.TryGetValue(normalized, out var pending) || pending.ExpiresUtc < DateTime.UtcNow)
        {
            Pending.TryRemove(normalized, out _);
            return (false, "The verification code expired. Request a new code.");
        }

        if (!string.Equals(pending.Code, code.Trim(), StringComparison.Ordinal))
            return (false, "The verification code is incorrect.");

        var employee = await _employees.GetByEmailAsync(normalized);
        if (employee is null)
            return (false, "No employee account is registered with that email.");

        employee.Password = newPassword.Trim();
        await _employees.UpdateAsync(employee);
        Pending.TryRemove(normalized, out _);
        return (true, "Your password was updated. You can sign in with the new password.");
    }

    private sealed record PendingReset(string Code, DateTime ExpiresUtc);
}
