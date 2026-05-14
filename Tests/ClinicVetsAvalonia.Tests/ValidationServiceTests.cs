using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Tests;

public class ValidationServiceTests
{
    [Theory]
    [InlineData("3761234")]
    [InlineData("3760000")]
    [InlineData("3769999")]
    public void ChipNumber_WithIsraeliPrefixAndFourDigits_IsValid(string chipNumber)
    {
        Assert.True(ValidationService.IsValidChipNumber(chipNumber));
    }

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

    [Theory]
    [InlineData(0.1)]
    [InlineData(25)]
    [InlineData(100)]
    public void Weight_WithinAllowedRange_IsValid(double weight)
    {
        Assert.True(ValidationService.IsValidWeight(weight));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100.1)]
    [InlineData(-1)]
    public void Weight_OutsideAllowedRange_IsInvalid(double weight)
    {
        Assert.False(ValidationService.IsValidWeight(weight));
    }
}
