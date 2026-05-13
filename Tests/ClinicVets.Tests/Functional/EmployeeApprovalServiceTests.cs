using ClinicVets.Application.Services;
using ClinicVets.Application.Validation;
using ClinicVets.Core.Entities;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public class EmployeeApprovalServiceTests
{
    [Fact]
    public async Task ApproveAsync_assigns_auto_id_sets_approved_and_applies_final_role()
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
            RequestedRole = "Secretary",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(pending);

        var sut = new EmployeeApprovalService(repo);
        var (ok, message) = await sut.ApproveAsync(pending.Id, "Veterinarian", admin);

        Assert.True(ok);
        Assert.Contains("1001", message, StringComparison.Ordinal);
        var updated = await repo.GetByIdAsync(pending.Id);
        Assert.NotNull(updated);
        Assert.Equal(EmployeeAccountStatusNames.Approved, updated.Status);
        Assert.Equal("1001", updated.EmployeeId);
        Assert.Equal("Veterinarian", updated.Role);
        Assert.Equal("Secretary", updated.RequestedRole);
    }

    [Fact]
    public async Task ApproveAsync_skips_occupied_ids_in_order()
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
            EmployeeId = "1001"
        });
        var pending = new Employee
        {
            FullName = "P",
            Email = "p@x.com",
            Password = "Abcd1234!",
            Role = "Veterinarian",
            RequestedRole = "Veterinarian",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(pending);
        var sut = new EmployeeApprovalService(repo);

        var (ok, message) = await sut.ApproveAsync(pending.Id, "Secretary", admin);

        Assert.True(ok);
        Assert.Contains("1002", message, StringComparison.Ordinal);
        var updated = await repo.GetByIdAsync(pending.Id);
        Assert.NotNull(updated);
        Assert.Equal("1002", updated.EmployeeId);
    }

    [Fact]
    public async Task ApproveAsync_multiple_pending_get_sequential_ids()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee
        {
            FullName = "Admin",
            Email = "adm@x.com",
            Password = "x",
            Role = "Admin",
            Status = EmployeeAccountStatusNames.Approved,
            EmployeeId = "9000"
        };
        await repo.AddAsync(admin);
        var p1 = new Employee
        {
            FullName = "One",
            Email = "one@x.com",
            Password = "Abcd1234!",
            Role = "Secretary",
            Status = EmployeeAccountStatusNames.Pending
        };
        var p2 = new Employee
        {
            FullName = "Two",
            Email = "two@x.com",
            Password = "Abcd1234!",
            Role = "Secretary",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(p1);
        await repo.AddAsync(p2);
        var sut = new EmployeeApprovalService(repo);

        Assert.True((await sut.ApproveAsync(p1.Id, "Secretary", admin)).Ok);
        Assert.True((await sut.ApproveAsync(p2.Id, "Veterinarian", admin)).Ok);

        Assert.Equal("1001", (await repo.GetByIdAsync(p1.Id))!.EmployeeId);
        Assert.Equal("1002", (await repo.GetByIdAsync(p2.Id))!.EmployeeId);
    }

    [Fact]
    public async Task ApproveAsync_fails_when_final_role_invalid()
    {
        var repo = new FakeEmployeeRepository();
        var admin = new Employee { FullName = "A", Email = "a@x.com", Password = "x", Role = "Admin" };
        await repo.AddAsync(admin);
        var pending = new Employee
        {
            FullName = "Odd",
            Email = "odd@x.com",
            Password = "Abcd1234!",
            Role = "Janitor",
            RequestedRole = "Janitor",
            Status = EmployeeAccountStatusNames.Pending
        };
        await repo.AddAsync(pending);
        var sut = new EmployeeApprovalService(repo);

        var (ok, message) = await sut.ApproveAsync(pending.Id, "Janitor", admin);

        Assert.False(ok);
        Assert.Contains("Final role", message, StringComparison.OrdinalIgnoreCase);
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

    [Fact]
    public void TryAllocateNext_returns_1001_when_no_ids_in_use_in_range()
    {
        var list = new[]
        {
            new Employee { EmployeeId = "9000" },
            new Employee { EmployeeId = "2001" }
        };
        Assert.Equal("1001", EmployeeIdAllocation.TryAllocateNext(list));
    }

    [Fact]
    public void TryAllocateNext_fills_smallest_gap_from_1001()
    {
        var list = new[]
        {
            new Employee { EmployeeId = "1001" },
            new Employee { EmployeeId = "1002" },
            new Employee { EmployeeId = "1004" }
        };
        Assert.Equal("1003", EmployeeIdAllocation.TryAllocateNext(list));
    }
}
