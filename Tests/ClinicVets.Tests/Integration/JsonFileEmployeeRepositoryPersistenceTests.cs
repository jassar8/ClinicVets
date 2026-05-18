using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Core.Entities;
using ClinicVets.Infrastructure.Repositories;

namespace ClinicVets.Tests.Integration;

public sealed class JsonFileEmployeeRepositoryPersistenceTests
{
    [Fact]
    public async Task Registered_employee_survives_repository_restart_and_login_by_username()
    {
        var dir = Path.Combine(Path.GetTempPath(), "ClinicVetsPersist_" + Guid.NewGuid().ToString("N"));
        try
        {
            var registration = new EmployeeRegistrationService(new JsonFileEmployeeRepository(dir));
            var (ok, message) = await registration.RegisterAsync(
                "New Vet",
                "persist.vet@clinic.test",
                "Abcd1234!",
                "Veterinarian",
                username: "vetpr01",
                autoApproveSelfRegistration: true);

            Assert.True(ok, message);

            var authAfterRestart = new EmployeeAuthenticationService(new JsonFileEmployeeRepository(dir));
            var (loginOk, _, employee) = await authAfterRestart.LoginAsync("vetpr01", "Abcd1234!");

            Assert.True(loginOk);
            Assert.NotNull(employee);
            Assert.Equal("persist.vet@clinic.test", employee.Email);
            Assert.Equal(EmployeeAccountStatusNames.Approved, employee.Status);
            Assert.True(employee.EmployeeId.Length == 4);
        }
        finally
        {
            try
            {
                Directory.Delete(dir, recursive: true);
            }
            catch
            {
                // Best-effort cleanup.
            }
        }
    }

}
