using System.Text;
using ClinicVets.Application.Services;
using ClinicVets.Infrastructure.Repositories;

namespace ClinicVets.Tests.Integration;

public sealed class JsonFileEmployeeRepositoryAdminBootstrapTests
{
    [Fact]
    public async Task Bootstrap_admin_login_works_after_legacy_bad_password_row()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ClinicVetsAuth_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, "employees.json");
        var legacy = """
                     [
                       {
                         "fullName": "Broken",
                         "email": "admin@clinicvets.com",
                         "password": "NotTheDemoPassword",
                         "role": "Admin",
                         "username": ""
                       },
                       {
                         "fullName": "Dr. Amir Levi",
                         "email": "vet@clinicvets.com",
                         "password": "Vet12!ab",
                         "role": "Veterinarian"
                       }
                     ]
                     """;
        File.WriteAllText(path, legacy, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));

        var repository = new JsonFileEmployeeRepository(dir);
        var auth = new EmployeeAuthenticationService(repository);

        var (ok, _, employee) = await auth.LoginAsync("admin", "Admin123!");

        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal("Admin", employee.Role);
        Assert.Equal("admin@clinicvets.com", employee.Email);
    }

    [Fact]
    public async Task Bootstrap_admin_login_works_with_email_alias()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ClinicVetsAuth_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "employees.json"), "[]", Encoding.UTF8);

        var repository = new JsonFileEmployeeRepository(dir);
        var auth = new EmployeeAuthenticationService(repository);

        var (ok, _, employee) = await auth.LoginAsync("  admin@clinicvets.com  ", "  Admin123!  ");

        Assert.True(ok);
        Assert.NotNull(employee);
        Assert.Equal("Admin", employee.Role);
    }
}
