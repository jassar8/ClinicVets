using ClinicVets.Application.Security;
using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public class EmployeeAuthenticationServiceTests
{
    [Fact]
    public async Task LoginAsync_fails_when_email_or_password_empty()
    {
        var sut = new EmployeeAuthenticationService(new FakeEmployeeRepository());

        var (ok, _, employee) = await sut.LoginAsync("", "x");

        Assert.False(ok);
        Assert.Null(employee);
    }

    [Fact]
    public async Task LoginAsync_fails_for_unknown_user()
    {
        var sut = new EmployeeAuthenticationService(new FakeEmployeeRepository());

        var (ok, message, employee) = await sut.LoginAsync("nobody@x.com", "secret");

        Assert.False(ok);
        Assert.Null(employee);
        Assert.Contains("Invalid", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_fails_when_password_wrong()
    {
        var repo = new FakeEmployeeRepository();
        var registration = new EmployeeRegistrationService(repo);
        await registration.RegisterAsync("User", "user@x.com", "Correct1!", "Secretary", username: "userx001");
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, _, employee) = await sut.LoginAsync("user@x.com", "wrong");

        Assert.False(ok);
        Assert.Null(employee);
    }

    [Fact]
    public async Task LoginAsync_blocks_pending_self_registered_employee()
    {
        var repo = new FakeEmployeeRepository();
        var registration = new EmployeeRegistrationService(repo);
        await registration.RegisterAsync(
            "Jane", "Jane@X.COM", "Valid1!ab", "Secretary",
            username: "janeusr1", autoApproveSelfRegistration: false);
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, message, employee) = await sut.LoginAsync("  jane@x.com  ", "Valid1!ab");

        Assert.False(ok);
        Assert.Null(employee);
        Assert.Contains("waiting for admin approval", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_succeeds_after_registration_is_approved_with_employee_id()
    {
        var repo = new FakeEmployeeRepository();
        var registration = new EmployeeRegistrationService(repo);
        await registration.RegisterAsync("Jane", "Jane@X.COM", "Valid1!ab", "Secretary", username: "janeusr2");
        var pending = await repo.GetByEmailAsync("jane@x.com");
        Assert.NotNull(pending);
        pending.Status = EmployeeAccountStatusNames.Approved;
        pending.EmployeeId = "4521";
        await repo.UpdateAsync(pending);

        var sut = new EmployeeAuthenticationService(repo);

        var (ok, _, employee) = await sut.LoginAsync("  jane@x.com  ", "Valid1!ab");

        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal("jane@x.com", employee.Email);
        Assert.Equal("Jane", employee.FullName);
        Assert.Equal("Secretary", employee.Role);
        Assert.Equal("4521", employee.EmployeeId);
    }

    [Fact]
    public async Task LoginAsync_blocks_rejected_employee()
    {
        var repo = new FakeEmployeeRepository();
        await repo.AddAsync(new Employee
        {
            FullName = "Rejected User",
            Email = "rej@x.com",
            Password = "Valid1!ab",
            Role = "Secretary",
            Status = EmployeeAccountStatusNames.Rejected,
            EmployeeId = string.Empty
        });
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, message, employee) = await sut.LoginAsync("rej@x.com", "Valid1!ab");

        Assert.False(ok);
        Assert.Null(employee);
        Assert.Contains("rejected", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task LoginAsync_succeeds_with_username_when_approved()
    {
        var repo = new FakeEmployeeRepository();
        await repo.AddAsync(new Employee
        {
            FullName = "Root",
            Username = "rootuser",
            Email = "root@x.com",
            Password = "Valid1!a",
            Role = "Veterinarian",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "3311"
        });
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, _, employee) = await sut.LoginAsync("rootuser", "Valid1!a");

        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal("root@x.com", employee.Email);
    }

    [Fact]
    public async Task LoginAsync_accepts_default_admin_username_or_email()
    {
        var repo = new FakeEmployeeRepository();
        await repo.AddAsync(new Employee
        {
            FullName = SystemAccounts.DefaultAdminDisplayName,
            Username = SystemAccounts.DefaultAdminUsername,
            Email = SystemAccounts.DefaultAdminEmail,
            Password = SystemAccounts.DefaultAdminPassword,
            Role = SystemAccounts.DefaultAdminRole
        });
        var sut = new EmployeeAuthenticationService(repo);

        var (byUser, _, userEmployee) = await sut.LoginAsync("admin", SystemAccounts.DefaultAdminPassword);
        var (byEmail, _, emailEmployee) =
            await sut.LoginAsync(SystemAccounts.DefaultAdminEmail, SystemAccounts.DefaultAdminPassword);

        Assert.True(byUser);
        Assert.True(byEmail);
        Assert.NotNull(userEmployee);
        Assert.NotNull(emailEmployee);
        Assert.Equal(SystemAccounts.DefaultAdminEmail, userEmployee.Email);
        Assert.Equal(SystemAccounts.DefaultAdminEmail, emailEmployee.Email);
    }

    [Fact]
    public async Task LoginAsync_succeeds_for_bootstrap_admin_alias_when_username_not_stored()
    {
        var repo = new FakeEmployeeRepository();
        await repo.AddAsync(new Employee
        {
            FullName = SystemAccounts.DefaultAdminDisplayName,
            Email = SystemAccounts.DefaultAdminEmail,
            Password = SystemAccounts.DefaultAdminPassword,
            Role = SystemAccounts.DefaultAdminRole,
            Username = string.Empty
        });
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, _, employee) = await sut.LoginAsync("admin", $"  {SystemAccounts.DefaultAdminPassword}  ");

        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal(SystemAccounts.DefaultAdminRole, employee.Role);
    }
}
