using System;
using System.Threading.Tasks;
using ClinicVetsAvalonia.Services;
using Xunit;

namespace ClinicVets.Avalonia.Unit;

public class PasswordResetServiceTests
{
    [Fact]
    public void GenerateCode_is_six_digits()
    {
        var code = PasswordResetService.GenerateCode();
        Assert.Equal(6, code.Length);
        Assert.True(int.TryParse(code, out var n));
        Assert.InRange(n, 100000, 999999);
    }

    [Fact]
    public async Task SendResetCodeAsync_without_env_returns_demo_message_with_code()
    {
        Environment.SetEnvironmentVariable("CLINIC_GMAIL_ADDRESS", null);
        Environment.SetEnvironmentVariable("CLINIC_GMAIL_APP_PASSWORD", null);

        var msg = await PasswordResetService.SendResetCodeAsync("any@test.local", "123456");
        Assert.Contains("מצב דמו", msg);
        Assert.Contains("123456", msg);
    }
}
