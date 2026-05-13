using ClinicVetsAvalonia.Services;
using Xunit;

namespace ClinicVets.Avalonia.Unit;

/// <summary>
/// White-box checks for Avalonia app validation rules (no UI, no SQLite file).
/// </summary>
public class ValidationServiceTests
{
    [Theory]
    [InlineData("abcdef", true)]
    [InlineData("admin1", true)]
    [InlineData("admin12", true)]
    [InlineData("admin", false)]
    [InlineData("admin12345", false)]
    [InlineData("ab12cd", true)]
    [InlineData("ab123cd", false)]
    public void Username_rules(string username, bool expected) =>
        Assert.Equal(expected, ValidationService.IsValidUsername(username));

    [Theory]
    [InlineData("Passw0rd!", true)]
    [InlineData("short1!", false)]
    [InlineData("NoDigit!!", false)]
    [InlineData("NoSpecial123", false)]
    public void Password_rules(string password, bool expected) =>
        Assert.Equal(expected, ValidationService.IsValidPassword(password));

    [Theory]
    [InlineData("Secretary", true)]
    [InlineData("Vet", true)]
    [InlineData("Admin", false)]
    public void Role_rules(string role, bool expected) =>
        Assert.Equal(expected, ValidationService.IsValidRole(role));

    [Theory]
    [InlineData("1234", true)]
    [InlineData("123", false)]
    public void Employee_number(string n, bool expected) =>
        Assert.Equal(expected, ValidationService.IsValidEmployeeNumber(n));

    [Theory]
    [InlineData("a@b.co", true)]
    [InlineData("bad", false)]
    public void Email_basic(string email, bool expected) =>
        Assert.Equal(expected, ValidationService.IsValidEmail(email));

    [Theory]
    [InlineData(0, true)]
    [InlineData(5, true)]
    [InlineData(-1, false)]
    public void Stock_quantity(int q, bool expected) =>
        Assert.Equal(expected, ValidationService.IsValidStockQuantity(q));

    [Theory]
    [InlineData(0.0, true)]
    [InlineData(10.5, true)]
    [InlineData(-0.01, false)]
    public void Money_non_negative(double m, bool expected) =>
        Assert.Equal(expected, ValidationService.IsValidMoney(m));

    [Fact]
    public void Required_text_nonempty() =>
        Assert.True(ValidationService.IsRequiredText("x"));

    [Fact]
    public void Required_text_empty_fails() =>
        Assert.False(ValidationService.IsRequiredText("   "));
}
