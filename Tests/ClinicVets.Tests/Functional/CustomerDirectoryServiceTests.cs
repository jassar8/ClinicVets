using ClinicVets.Application.Services;
using ClinicVets.Tests.Integration;

namespace ClinicVets.Tests.Functional;

public class CustomerDirectoryServiceTests
{
    [Fact]
    public async Task RegisterCustomerAsync_succeeds()
    {
        var repo = new FakeCustomerDirectoryRepository();
        var sut = new CustomerDirectoryService(repo);

        var (ok, msg) = await sut.RegisterCustomerAsync("Noa Levi", "123456789", "0525550101", "noa@example.com");

        Assert.True(ok);
        Assert.Contains("success", msg, StringComparison.OrdinalIgnoreCase);
        var list = await repo.GetAllCustomersAsync();
        Assert.Single(list);
    }

    [Fact]
    public async Task RegisterCustomerAsync_fails_on_duplicate_national_id()
    {
        var repo = new FakeCustomerDirectoryRepository();
        var sut = new CustomerDirectoryService(repo);
        await sut.RegisterCustomerAsync("Noa Levi", "123456789", "0525550101", "noa@example.com");

        var (ok, msg) = await sut.RegisterCustomerAsync("Other Name", "123456789", "0525550102", "other@example.com");

        Assert.False(ok);
        Assert.Contains("national ID", msg, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SearchCustomersAsync_matches_email_token()
    {
        var repo = new FakeCustomerDirectoryRepository();
        var sut = new CustomerDirectoryService(repo);
        await sut.RegisterCustomerAsync("Noa Levi", "987654321", "0525550101", "noa.unique@example.com");

        var hits = await sut.SearchCustomersAsync("unique");

        Assert.Single(hits);
        Assert.Contains("noa.unique", hits[0].Email, StringComparison.Ordinal);
    }
}
