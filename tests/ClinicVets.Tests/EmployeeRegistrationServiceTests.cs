using ClinicVets.Application.Services;
using ClinicVets.Tests.Fakes;

namespace ClinicVets.Tests;

public class EmployeeRegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_succeeds_for_new_email()
    {
        var repo = new FakeEmployeeRepository();
        var sut = new EmployeeRegistrationService(repo);

        var (ok, message) = await sut.RegisterAsync("Test User", "new@x.com", "Abcd1234!", "Secretary");

        Assert.True(ok);
        Assert.Contains("success", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_fails_when_email_already_exists()
    {
        var repo = new FakeEmployeeRepository();
        var sut = new EmployeeRegistrationService(repo);
        await sut.RegisterAsync("Alice", "dup@x.com", "Abcd1234!", "Administrator");

        var (ok, message) = await sut.RegisterAsync("Bob", "dup@x.com", "Efgh5678@", "Secretary");

        Assert.False(ok);
        Assert.Contains("already", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_fails_for_invalid_email()
    {
        var sut = new EmployeeRegistrationService(new FakeEmployeeRepository());

        var (ok, message) = await sut.RegisterAsync("Test User", "not-an-email", "Abcd1234!", "Secretary");

        Assert.False(ok);
        Assert.Contains("email", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_fails_for_weak_password()
    {
        var sut = new EmployeeRegistrationService(new FakeEmployeeRepository());

        var (ok, message) = await sut.RegisterAsync("Test User", "ok@mail.com", "weak", "Secretary");

        Assert.False(ok);
        Assert.Contains("Password", message, StringComparison.OrdinalIgnoreCase);
    }
}
