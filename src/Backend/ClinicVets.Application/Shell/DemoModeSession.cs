using ClinicVets.Application.Security;
using ClinicVets.Core;
using ClinicVets.Core.Entities;

namespace ClinicVets.Application.Shell;

/// <summary>Tracks quick-access demo workspace (in-memory data, not a real authenticated session).</summary>
public static class DemoModeSession
{
    private static Employee? _cachedEffective;
    private static Employee? _cachedSource;
    private static EmployeeRole? _cachedSimulated;

    public static bool IsActive { get; private set; }

    /// <summary>When set while <see cref="IsActive"/> is true, permission checks use this role instead of the signed-in employee.</summary>
    public static EmployeeRole? SimulatedRole { get; private set; }

    public static void Enter()
    {
        IsActive = true;
        SimulatedRole = null;
        InvalidateEffectiveEmployeeCache();
    }

    public static void Exit()
    {
        IsActive = false;
        SimulatedRole = null;
        InvalidateEffectiveEmployeeCache();
    }

    /// <summary>Sets the simulated clinic role for Demo Mode UI only.</summary>
    public static void SetSimulatedRole(EmployeeRole role)
    {
        if (!IsActive)
            return;
        SimulatedRole = role;
        InvalidateEffectiveEmployeeCache();
    }

    public static void InvalidateEffectiveEmployeeCache()
    {
        _cachedEffective = null;
        _cachedSource = null;
        _cachedSimulated = null;
    }

    /// <summary>Returns the employee record used for RBAC in the shell (clone with overridden role in Demo Mode).</summary>
    public static Employee GetEffectiveEmployee(Employee sessionEmployee)
    {
        ArgumentNullException.ThrowIfNull(sessionEmployee);
        if (!IsActive)
            return sessionEmployee;

        if (!SimulatedRole.HasValue)
            return sessionEmployee;

        if (_cachedSource == sessionEmployee &&
            _cachedSimulated == SimulatedRole &&
            _cachedEffective is not null)
            return _cachedEffective;

        _cachedSource = sessionEmployee;
        _cachedSimulated = SimulatedRole;
        _cachedEffective = CloneWithRole(sessionEmployee, EmployeeRoleNames.ToStoredString(SimulatedRole.Value));
        return _cachedEffective;
    }

    private static Employee CloneWithRole(Employee e, string role) => new()
    {
        Id = e.Id,
        FullName = e.FullName,
        Username = e.Username,
        Email = e.Email,
        Password = e.Password,
        Role = role,
        RequestedRole = e.RequestedRole,
        Status = e.Status,
        EmployeeId = e.EmployeeId
    };
}
