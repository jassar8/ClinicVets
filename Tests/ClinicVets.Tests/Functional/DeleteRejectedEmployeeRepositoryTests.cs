using ClinicVets.Application.Security;
using ClinicVets.Core.Entities;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public sealed class DeleteRejectedEmployeeRepositoryTests
{
    [Fact]
    public async Task DeleteRejectedEmployeeAsync_removes_rejected_row()
    {
        var repo = new FakeEmployeeRepository();
        var id = Guid.Parse("11111111-2222-3333-4444-555555555555");
        await repo.AddAsync(new Employee
        {
            Id = id,
            FullName = "Rejected Row",
            Email = "rejected-delete-test@local",
            Role = EmployeeRoleNames.Secretary,
            RequestedRole = EmployeeRoleNames.Secretary,
            Status = EmployeeAccountStatusNames.Rejected
        });

        Assert.True(await repo.DeleteRejectedEmployeeAsync(id));
        Assert.Null(await repo.GetByIdAsync(id));
        var all = await repo.GetAllAsync();
        Assert.DoesNotContain(all, e => e.Id == id);
    }

    [Fact]
    public async Task DeleteRejectedEmployeeAsync_returns_false_for_approved()
    {
        var repo = new FakeEmployeeRepository();
        var id = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee");
        await repo.AddAsync(new Employee
        {
            Id = id,
            FullName = "Approved Row",
            Email = "approved-delete-test@local",
            Role = EmployeeRoleNames.Secretary,
            RequestedRole = EmployeeRoleNames.Secretary,
            Status = EmployeeAccountStatusNames.Approved
        });

        Assert.False(await repo.DeleteRejectedEmployeeAsync(id));
        Assert.NotNull(await repo.GetByIdAsync(id));
    }

    [Fact]
    public async Task DeleteRejectedEmployeeAsync_returns_false_for_pending()
    {
        var repo = new FakeEmployeeRepository();
        var id = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff");
        await repo.AddAsync(new Employee
        {
            Id = id,
            FullName = "Pending Row",
            Email = "pending-delete-test@local",
            Role = EmployeeRoleNames.Secretary,
            RequestedRole = EmployeeRoleNames.Secretary,
            Status = EmployeeAccountStatusNames.Pending
        });

        Assert.False(await repo.DeleteRejectedEmployeeAsync(id));
        Assert.NotNull(await repo.GetByIdAsync(id));
    }
}
