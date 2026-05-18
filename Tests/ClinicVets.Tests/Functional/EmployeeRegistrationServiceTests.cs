using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public class EmployeeRegistrationServiceTests
{
    [Fact]
    public async Task RegisterAsync_succeeds_for_new_email_and_sets_pending_when_auto_approve_off()
    {
        var repo = new FakeEmployeeRepository();
        var sut = new EmployeeRegistrationService(repo);

        var (ok, message) = await sut.RegisterAsync(
            "Test User", "new@x.com", "Abcd1234!", "Secretary",
            username: "newuser1", autoApproveSelfRegistration: false);

        Assert.True(ok);
        Assert.Contains("administrator", message, StringComparison.OrdinalIgnoreCase);
        var saved = await repo.GetByEmailAsync("new@x.com");
        Assert.NotNull(saved);
        Assert.Equal(EmployeeAccountStatusNames.Pending, saved.Status);
        Assert.Equal(string.Empty, saved.EmployeeId);
        Assert.Equal("newuser1", saved.Username);
    }

    [Fact]
    public async Task RegisterAsync_auto_approves_and_assigns_id_on_desktop()
    {
        var repo = new FakeEmployeeRepository();
        var sut = new EmployeeRegistrationService(repo);

        var (ok, message) = await sut.RegisterAsync(
            "Desk User", "desk@x.com", "Abcd1234!", "Secretary",
            username: "deskusr", autoApproveSelfRegistration: true);

        Assert.True(ok);
        Assert.Contains("sign in", message, StringComparison.OrdinalIgnoreCase);
        var saved = await repo.GetByEmailAsync("desk@x.com");
        Assert.NotNull(saved);
        Assert.Equal(EmployeeAccountStatusNames.Approved, saved.Status);
        Assert.Equal("deskusr", saved.Username);
        Assert.True(saved.EmployeeId.Length == 4);
    }

    [Fact]
    public async Task RegisterAsync_fails_when_pending_email_already_registered()
    {
        var repo = new FakeEmployeeRepository();
        var sut = new EmployeeRegistrationService(repo);
        await sut.RegisterAsync(
            "Alice", "dup@x.com", "Abcd1234!", "Secretary",
            username: "alice01", autoApproveSelfRegistration: false);

        var (ok, message) = await sut.RegisterAsync(
            "Bob", "dup@x.com", "Efgh5678@", "Secretary",
            username: "bobuser1", autoApproveSelfRegistration: false);

        Assert.False(ok);
        Assert.Contains("waiting", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_fails_when_approved_email_already_exists()
    {
        var repo = new FakeEmployeeRepository();
        await repo.AddAsync(new Employee
        {
            FullName = "Existing",
            Email = "taken@x.com",
            Password = "Abcd1234!",
            Role = "Secretary",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "1001"
        });
        var sut = new EmployeeRegistrationService(repo);

        var (ok, message) = await sut.RegisterAsync(
            "Other", "taken@x.com", "Efgh5678@", "Secretary", username: "otherusr");

        Assert.False(ok);
        Assert.Contains("already", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_rejects_administrator_role_for_self_service()
    {
        var sut = new EmployeeRegistrationService(new FakeEmployeeRepository());

        var (ok, message) = await sut.RegisterAsync(
            "Eve", "eve@x.com", "Abcd1234!", "Admin", username: "eveuser1");

        Assert.False(ok);
        Assert.Contains("self-registration", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_fails_for_invalid_email()
    {
        var sut = new EmployeeRegistrationService(new FakeEmployeeRepository());

        var (ok, message) = await sut.RegisterAsync(
            "Test User", "not-an-email", "Abcd1234!", "Secretary", username: "testusr1");

        Assert.False(ok);
        Assert.Contains("email", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_fails_for_weak_password()
    {
        var sut = new EmployeeRegistrationService(new FakeEmployeeRepository());

        var (ok, message) = await sut.RegisterAsync(
            "Test User", "ok@mail.com", "weak", "Secretary", username: "weakusr1");

        Assert.False(ok);
        Assert.Contains("Password", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_allows_admin_role_when_acting_admin_with_employee_id()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee
        {
            FullName = "Admin User",
            Email = "admin@x.com",
            Password = "Admin1!zz",
            Role = "Admin",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "9000"
        };
        await repo.AddAsync(admin);
        var sut = new EmployeeRegistrationService(repo);

        var (ok, message) = await sut.RegisterAsync("Neo", "neo@x.com", "Abcd1234!", "Admin", admin, null, "7777");

        Assert.True(ok);
        Assert.Contains("success", message, StringComparison.OrdinalIgnoreCase);
        var neo = await repo.GetByEmailAsync("neo@x.com");
        Assert.NotNull(neo);
        Assert.Equal(EmployeeAccountStatusNames.Approved, neo.Status);
        Assert.Equal("7777", neo.EmployeeId);
    }

    [Fact]
    public async Task RegisterAsync_admin_path_requires_four_digit_employee_id()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee
        {
            FullName = "Admin User",
            Email = "admin2@x.com",
            Password = "Admin1!zz",
            Role = "Admin",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "9001"
        };
        await repo.AddAsync(admin);
        var sut = new EmployeeRegistrationService(repo);

        var (ok, message) = await sut.RegisterAsync("Sam", "sam@x.com", "Abcd1234!", "Secretary", admin, null, "12");

        Assert.False(ok);
        Assert.Contains("four", message, StringComparison.OrdinalIgnoreCase);
    }
}
