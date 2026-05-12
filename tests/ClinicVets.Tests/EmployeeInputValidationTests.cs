using ClinicVets.Application.Validation;

namespace ClinicVets.Tests;

public class EmployeeInputValidationTests
{
    [Theory]
    [InlineData("a@b.co", true)]
    [InlineData("bad", false)]
    [InlineData("missingdot@x", false)]
    public void IsValidEmail_matches_expected(string email, bool expected)
    {
        Assert.Equal(expected, EmployeeInputValidation.IsValidEmail(email));
    }

    [Theory]
    [InlineData("Abcd1234!", true)]
    [InlineData("Short1!", false)]
    [InlineData("NoDigits!!", false)]
    [InlineData("NoSpecial12", false)]
    public void IsValidPassword_matches_expected(string password, bool expected)
    {
        Assert.Equal(expected, EmployeeInputValidation.IsValidPassword(password));
    }
}
