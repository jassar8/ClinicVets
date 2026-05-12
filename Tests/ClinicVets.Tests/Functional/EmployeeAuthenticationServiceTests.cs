using ClinicVets.Application.Services;
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
        await registration.RegisterAsync("Jane", "Jane@X.COM", "Valid1!ab", "Administrator");
        var sut = new EmployeeAuthenticationService(repo);

        var (ok, _, employee) = await sut.LoginAsync("  jane@x.com  ", "Valid1!ab");

        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal("jane@x.com", employee.Email);
        Assert.Equal("Jane", employee.FullName);
    }
}
