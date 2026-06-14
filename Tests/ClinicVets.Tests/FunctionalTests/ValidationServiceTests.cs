using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Tests;

// Unit tests for the validation rules. Each rule is checked with both valid and invalid input.
// The "invalid" tests assert that bad input is correctly rejected, so they still PASS.
public class ValidationServiceTests
{
    // ---------- Chip number: 7 digits, "376" prefix ----------

    // Positive: well-formed Israeli chip numbers are accepted.
    [Theory]
    [InlineData("3761234")]
    [InlineData("3760000")]
    [InlineData("3769999")]
    public void ChipNumber_WithIsraeliPrefixAndFourDigits_IsValid(string chipNumber)
    {
        Assert.True(ValidationService.IsValidChipNumber(chipNumber));
    }

    // Negative: wrong length, wrong prefix, letters, or empty are rejected.
    [Theory]
    [InlineData("376123")]
    [InlineData("37612345")]
    [InlineData("1231234")]
    [InlineData("37612A4")]
    [InlineData("")]
    public void ChipNumber_OutsideRequiredBoundary_IsInvalid(string chipNumber)
    {
        Assert.False(ValidationService.IsValidChipNumber(chipNumber));
    }

    // ---------- Weight: allowed range 0.1 - 100 kg ----------

    // Positive: weights at and within the boundaries are accepted.
    [Theory]
    [InlineData(0.1)]
    [InlineData(25)]
    [InlineData(100)]
    public void Weight_WithinAllowedRange_IsValid(double weight)
    {
        Assert.True(ValidationService.IsValidWeight(weight));
    }

    // Negative: zero, above the max, or negative weights are rejected.
    [Theory]
    [InlineData(0)]
    [InlineData(100.1)]
    [InlineData(-1)]
    public void Weight_OutsideAllowedRange_IsInvalid(double weight)
    {
        Assert.False(ValidationService.IsValidWeight(weight));
    }

    // ---------- Email: single "@" and an allowed suffix ----------

    // Positive: emails ending in an allowed suffix (.com, .co.il, .net, .org, .il) are accepted.
    [Theory]
    [InlineData("user@gmail.com")]
    [InlineData("user@example.co.il")]
    [InlineData("admin@clinic.com")]
    [InlineData("test@domain.org")]
    [InlineData("name@site.net")]
    [InlineData("info@company.il")]
    public void Email_WithAllowedDomainSuffix_IsValid(string email)
    {
        Assert.True(ValidationService.IsValidEmail(email));
        Assert.Null(ValidationService.GetEmailValidationMessage(email));
    }

    // Negative: missing suffix/"@", disallowed suffix, or empty are rejected with a message.
    [Theory]
    [InlineData("user@gmail")]
    [InlineData("user@host.edu")]
    [InlineData("missing-at.com")]
    [InlineData("")]
    [InlineData("a@b.c")]
    public void Email_WithInvalidDomainSuffix_IsInvalid(string email)
    {
        Assert.False(ValidationService.IsValidEmail(email));
        Assert.NotNull(ValidationService.GetEmailValidationMessage(email));
    }

    // ---------- Birth date: in the past and not before year 2000 ----------

    // Positive: yesterday is a valid birth date.
    [Fact]
    public void BirthDate_Yesterday_IsValid()
    {
        var birthDate = DateTime.Today.AddDays(-1);

        Assert.True(ValidationService.IsValidBirthDate(birthDate));
        Assert.Null(ValidationService.GetBirthDateValidationMessage(birthDate));
    }

    // Positive: the year-2000 lower boundary is valid.
    [Fact]
    public void BirthDate_Year2000_IsValid()
    {
        var birthDate = new DateTime(2000, 1, 1);

        Assert.True(ValidationService.IsValidBirthDate(birthDate));
        Assert.Null(ValidationService.GetBirthDateValidationMessage(birthDate));
    }

    // Negative: a future birth date is rejected with the correct message.
    [Fact]
    public void BirthDate_Tomorrow_IsInvalid()
    {
        var birthDate = DateTime.Today.AddDays(1);

        Assert.False(ValidationService.IsValidBirthDate(birthDate));
        Assert.Equal("תאריך לידה חייב להיות בעבר", ValidationService.GetBirthDateValidationMessage(birthDate));
    }

    // Negative: a birth date before year 2000 is rejected with the correct message.
    [Fact]
    public void BirthDate_BeforeYear2000_IsInvalid()
    {
        var birthDate = new DateTime(1999, 12, 31);

        Assert.False(ValidationService.IsValidBirthDate(birthDate));
        Assert.Equal("תאריך לידה לא יכול להיות לפני שנת 2000", ValidationService.GetBirthDateValidationMessage(birthDate));
    }
}
