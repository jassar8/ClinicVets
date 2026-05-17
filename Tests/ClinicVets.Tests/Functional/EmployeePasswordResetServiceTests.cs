using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public sealed class EmployeePasswordResetServiceTests
{
    [Fact]
    public async Task ResetPassword_UpdatesStoredPassword_WhenCodeValid()
    {
        var repo = new FakeEmployeeRepository();
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Email = "vet@clinicvets.com",
            Username = "vet",
            Password = "Old12!ab",
            FullName = "Demo Vet",
            Role = "Veterinarian",
            Status = "Approved",
            EmployeeId = "1001"
        };
        await repo.AddAsync(employee);

        var reset = new EmployeePasswordResetService(repo);
        var request = await reset.RequestCodeAsync(employee.Email);
        Assert.True(request.IsSuccess);

        var code = ExtractCodeFromDemoMessage(request.Message);
        Assert.False(string.IsNullOrWhiteSpace(code));

        var apply = await reset.ResetPasswordAsync(employee.Email, code, "New12!xy");
        Assert.True(apply.IsSuccess);

        var auth = new EmployeeAuthenticationService(repo);
        var login = await auth.LoginAsync(employee.Email, "New12!xy");
        Assert.True(login.IsSuccess);
    }

    [Fact]
    public async Task ResetPassword_RejectsWrongCode()
    {
        var repo = new FakeEmployeeRepository();
        await repo.AddAsync(new Employee
        {
            Id = Guid.NewGuid(),
            Email = "sec@clinicvets.com",
            Username = "sec",
            Password = "Sec12!ab",
            FullName = "Secretary",
            Role = "Secretary",
            Status = "Approved",
            EmployeeId = "1002"
        });

        var reset = new EmployeePasswordResetService(repo);
        await reset.RequestCodeAsync("sec@clinicvets.com");
        var apply = await reset.ResetPasswordAsync("sec@clinicvets.com", "000000", "New12!xy");
        Assert.False(apply.IsSuccess);
    }

    private static string ExtractCodeFromDemoMessage(string message)
    {
        const string marker = "verification code is ";
        var idx = message.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        if (idx < 0)
            return string.Empty;
        return message[(idx + marker.Length)..].Trim();
    }
}
