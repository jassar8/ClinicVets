using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public class EmployeeApprovalWorkflowTests
{
    [Fact]
    public async Task Register_pending_then_approve_then_login_succeeds()
    {
        var repo = new FakeEmployeeRepository();
        var registration = new EmployeeRegistrationService(repo);
        var auth = new EmployeeAuthenticationService(repo);
        var approvals = new EmployeeApprovalService(repo);

        var admin = new Employee
        {
            FullName = "Admin",
            Email = "admin@wf.com",
            Password = "Admin1!zz",
            Role = "Admin",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "9000"
        };
        await repo.AddAsync(admin);

        var (regOk, _) = await registration.RegisterAsync("Pat Lee", "pat@wf.com", "Abcd1234!", "Secretary");
        Assert.True(regOk);

        var (blocked, pendingMsg, _) = await auth.LoginAsync("pat@wf.com", "Abcd1234!");
        Assert.False(blocked);
        Assert.Contains("waiting for admin approval", pendingMsg, StringComparison.OrdinalIgnoreCase);

        var pending = (await approvals.GetPendingAsync()).Single();
        Assert.Equal(EmployeeAccountStatusNames.Pending, pending.Status);
        Assert.Equal(string.Empty, pending.EmployeeId.Trim());
        Assert.Equal("Secretary", pending.Role);

        var (approveOk, approveMsg) = await approvals.ApproveAsync(pending.Id, "5511", admin);
        Assert.True(approveOk);
        Assert.Contains("approved", approveMsg, StringComparison.OrdinalIgnoreCase);

        var (ok, _, employee) = await auth.LoginAsync("pat@wf.com", "Abcd1234!");
        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal(EmployeeAccountStatusNames.Approved, employee.Status);
        Assert.Equal("5511", employee.EmployeeId);
    }

    [Fact]
    public async Task Rejected_user_cannot_log_in()
    {
        var repo = new FakeEmployeeRepository();
        var registration = new EmployeeRegistrationService(repo);
        var auth = new EmployeeAuthenticationService(repo);
        var approvals = new EmployeeApprovalService(repo);

        var admin = new Employee
        {
            FullName = "Admin",
            Email = "admin2@wf.com",
            Password = "Admin1!zz",
            Role = "Admin",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "9001"
        };
        await repo.AddAsync(admin);

        await registration.RegisterAsync("Rex", "rex@wf.com", "Abcd1234!", "Veterinarian");
        var pending = (await approvals.GetPendingAsync()).Single();

        var (rejOk, _) = await approvals.RejectAsync(pending.Id, admin);
        Assert.True(rejOk);

        var (loginOk, msg, _) = await auth.LoginAsync("rex@wf.com", "Abcd1234!");
        Assert.False(loginOk);
        Assert.Contains("rejected", msg, StringComparison.OrdinalIgnoreCase);
    }
}
