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

        var (ok, message) = await sut.RegisterAsync("Test User", "new@x.com", "Pass1!", "Secretary");

        Assert.True(ok);
        Assert.Contains("success", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_fails_when_email_already_exists()
    {
        var repo = new FakeEmployeeRepository();
        var sut = new EmployeeRegistrationService(repo);
        await sut.RegisterAsync("A", "dup@x.com", "p", "Administrator");

        var (ok, message) = await sut.RegisterAsync("B", "dup@x.com", "p2", "Secretary");

        Assert.False(ok);
        Assert.Contains("already", message, StringComparison.OrdinalIgnoreCase);
    }
}
