using ClinicVets.Application.Security;
using ClinicVets.Application.Validation;
using ClinicVets.Core.Entities;

namespace ClinicVets.Tests.Functional;

public class CustomerInputValidationTests
{
    [Theory]
    [InlineData("Anna", true)]
    [InlineData("Mary-Jane O'Brien", true)]
    [InlineData("A", false)]
    [InlineData("Anna2", false)]
    [InlineData("Anna@", false)]
    public void IsValidCustomerFullName_rules(string name, bool expected) =>
        Assert.Equal(expected, CustomerInputValidation.IsValidCustomerFullName(name));

    [Theory]
    [InlineData("123456789", true)]
    [InlineData("12345678", false)]
    [InlineData("1234567890", false)]
    [InlineData("abcdefghi", false)]
    public void IsValidNationalId_rules(string id, bool expected) =>
        Assert.Equal(expected, CustomerInputValidation.IsValidNationalId(id));

    [Theory]
    [InlineData("+1 (555) 010-2030", true)]
    [InlineData("0525550142", true)]
    [InlineData("12345", false)]
    public void IsValidCustomerPhone_rules(string phone, bool expected) =>
        Assert.Equal(expected, CustomerInputValidation.IsValidCustomerPhone(phone));

    [Theory]
    [InlineData("a@b.co", true)]
    [InlineData("not-an-email", false)]
    [InlineData("@nodomain.com", false)]
    public void IsValidCustomerEmail_rules(string email, bool expected) =>
        Assert.Equal(expected, CustomerInputValidation.IsValidCustomerEmail(email));
}
