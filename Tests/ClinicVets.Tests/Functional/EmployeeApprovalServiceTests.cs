using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public class EmployeeApprovalServiceTests
{
    [Fact]
    public async Task ApproveAsync_assigns_id_and_sets_approved()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee
        {
            FullName = "Admin",
            Email = "a@x.com",
            Password = "x",
            Role = "Admin",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "9000"
        };
        await repo.AddAsync(admin);
        var pending = new Employee
        {
            FullName = "New Hire",
            Email = "hire@x.com",
            Password = "Abcd1234!",
            Role = "Secretary",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(pending);

        var sut = new EmployeeApprovalService(repo);
        var (ok, message) = await sut.ApproveAsync(pending.Id, "4820", admin);

        Assert.True(ok);
        Assert.Contains("approved", message, StringComparison.OrdinalIgnoreCase);
        var updated = await repo.GetByIdAsync(pending.Id);
        Assert.NotNull(updated);
        Assert.Equal(EmployeeAccountStatusNames.Approved, updated.Status);
        Assert.Equal("4820", updated.EmployeeId);
    }

    [Fact]
    public async Task ApproveAsync_fails_when_id_not_four_digits()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee { FullName = "A", Email = "a@x.com", Password = "x", Role = "Admin" };
        await repo.AddAsync(admin);
        var pending = new Employee
        {
            FullName = "P",
            Email = "p@x.com",
            Password = "Abcd1234!",
            Role = "Veterinarian",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(pending);
        var sut = new EmployeeApprovalService(repo);

        var (ok, message) = await sut.ApproveAsync(pending.Id, "482", admin);

        Assert.False(ok);
        Assert.Contains("four", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ApproveAsync_fails_when_id_duplicate()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee { FullName = "A", Email = "a@x.com", Password = "x", Role = "Admin" };
        await repo.AddAsync(admin);
        await repo.AddAsync(new Employee
        {
            FullName = "Existing",
            Email = "e@x.com",
            Password = "Abcd1234!",
            Role = "Secretary",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "1000"
        });
        var pending = new Employee
        {
            FullName = "P",
            Email = "p@x.com",
            Password = "Abcd1234!",
            Role = "Veterinarian",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(pending);
        var sut = new EmployeeApprovalService(repo);

        var (ok, message) = await sut.ApproveAsync(pending.Id, "1000", admin);

        Assert.False(ok);
        Assert.Contains("already", message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RejectAsync_sets_rejected()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee { FullName = "A", Email = "a@x.com", Password = "x", Role = "Admin" };
        await repo.AddAsync(admin);
        var pending = new Employee
        {
            FullName = "P",
            Email = "p@x.com",
            Password = "Abcd1234!",
            Role = "Veterinarian",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(pending);
        var sut = new EmployeeApprovalService(repo);

        var (ok, message) = await sut.RejectAsync(pending.Id, admin);

        Assert.True(ok);
        Assert.Contains("reject", message, StringComparison.OrdinalIgnoreCase);
        var updated = await repo.GetByIdAsync(pending.Id);
        Assert.NotNull(updated);
        Assert.Equal(EmployeeAccountStatusNames.Rejected, updated.Status);
        Assert.Equal(string.Empty, updated.EmployeeId);
    }
}
