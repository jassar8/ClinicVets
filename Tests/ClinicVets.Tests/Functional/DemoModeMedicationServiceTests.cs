using ClinicVets.Application.Services;
using ClinicVets.Application.Shell;
using ClinicVets.Desktop.Services;

namespace ClinicVets.Tests.Functional;

public class DemoModeMedicationServiceTests
{
    [Fact]
    public async Task TryEnterDemoMode_initializes_medication_service()
    {
        AppServices.Initialize();

        var ok = AppServices.TryEnterDemoMode(out _, out var error);
        Assert.True(ok, error);
        Assert.NotNull(AppServices.Medications);

        var items = await AppServices.Medications.SearchAsync(null, MedicationSearchFilter.FilterAll);
        Assert.NotEmpty(items);

        AppServices.ExitDemoMode();
        DemoModeSession.Exit();
    }
}
