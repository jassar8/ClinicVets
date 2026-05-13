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
        await registration.RegisterAsync("User", "user@x.com", "Correct1!", "Secretary");
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, _, employee) = await sut.LoginAsync("user@x.com", "wrong");

        Assert.False(ok);
        Assert.Null(employee);
    }

    [Fact]
    public async Task LoginAsync_succeeds_after_registration_with_normalized_email()
    {
        var repo = new FakeEmployeeRepository();
        var registration = new EmployeeRegistrationService(repo);
        await registration.RegisterAsync("Jane", "Jane@X.COM", "Valid1!ab", "Secretary");
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, _, employee) = await sut.LoginAsync("  jane@x.com  ", "Valid1!ab");

        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal("jane@x.com", employee.Email);
        Assert.Equal("Jane", employee.FullName);
        Assert.Equal("Secretary", employee.Role);
    }

    [Fact]
    public async Task LoginAsync_succeeds_with_username_when_configured()
    {
        var repo = new FakeEmployeeRepository();
        await repo.AddAsync(new ClinicVets.Core.Entities.Employee
        {
            FullName = "Root",
            Username = "rootuser",
            Email = "root@x.com",
            Password = "Valid1!a",
            Role = "Veterinarian"
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
}
