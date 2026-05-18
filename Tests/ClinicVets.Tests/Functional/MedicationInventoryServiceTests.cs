using ClinicVets.Application.Services;
using ClinicVets.Core.Entities;
using ClinicVets.Infrastructure.Repositories;

namespace ClinicVets.Tests.Functional;

public sealed class MedicationInventoryServiceTests : IDisposable
{
    private readonly string _root;
    private readonly MedicationInventoryService _service;

    public MedicationInventoryServiceTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ClinicVetsTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
        _service = new MedicationInventoryService(new JsonFileMedicationRepository(_root));
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_root))
                Directory.Delete(_root, recursive: true);
        }
        catch
        {
            // ignore cleanup races on CI
        }
    }

    [Fact]
    public async Task Add_Search_Update_Delete_RoundTrip()
    {
        var add = await _service.AddAsync("TestMed", 12, 9.5, DateTime.Today.AddMonths(4), "note");
        Assert.True(add.IsSuccess);

        var rows = await _service.SearchAsync("Test", MedicationSearchFilter.FilterAll);
        Assert.Contains(rows, m => m.Name == "TestMed");

        var item = rows.First(m => m.Name == "TestMed");
        var update = await _service.UpdateAsync(item.Id, 8, 10, DateTime.Today.AddMonths(5), "updated");
        Assert.True(update.IsSuccess);

        var delete = await _service.DeleteAsync(item.Id);
        Assert.True(delete.IsSuccess);

        var after = await _service.SearchAsync("TestMed", MedicationSearchFilter.FilterAll);
        Assert.DoesNotContain(after, m => m.Name == "TestMed");
    }

    [Fact]
    public async Task Add_RejectsDuplicateName()
    {
        Assert.True((await _service.AddAsync("Dup", 1, 1, DateTime.Today.AddMonths(1), "")).IsSuccess);
        var second = await _service.AddAsync("dup", 2, 2, DateTime.Today.AddMonths(1), "");
        Assert.False(second.IsSuccess);
    }
}
