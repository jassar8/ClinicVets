using System.Collections.Generic;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Tests;

// Functional tests for the login flow (AuthService).
// Note on "fail" tests: a negative test checks that login is correctly REJECTED for bad input.
// The test itself is still expected to PASS, because the app rejecting invalid input is the correct behavior.
public class LoginFunctionalTests
{
    // Shared fixture: one approved Secretary and one approved Vet to log in against.
    private static List<Employee> BuildEmployees() =>
    [
        new Employee
        {
            Username = "secuser",
            Password = "Sec123!a",
            EmployeeNumber = "1002",
            Email = "sec@clinic.com",
            IdNumber = "300000027",
            Role = "Secretary"
        },
        new Employee
        {
            Username = "vetuser",
            Password = "Vet123!a",
            EmployeeNumber = "1003",
            Email = "vet@clinic.com",
            IdNumber = "300000036",
            Role = "Vet"
        }
    ];

    // ---------- Positive tests (valid input should succeed) ----------

    // Positive: a Secretary's correct username and password should log in successfully.
    [Fact]
    public void Login_WithValidSecretaryCredentials_ShouldSucceed()
    {
        var result = AuthService.TryLogin("secuser", "Sec123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.NotNull(result.Employee);
        Assert.Equal("secuser", result.Employee!.Username);
        Assert.Equal(LoginFailureReason.None, result.Reason);
    }

    // Positive: a Vet's correct username and password should log in successfully.
    [Fact]
    public void Login_WithValidVetCredentials_ShouldSucceed()
    {
        var result = AuthService.TryLogin("vetuser", "Vet123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.NotNull(result.Employee);
        Assert.Equal("vetuser", result.Employee!.Username);
    }

    // ---------- Negative tests (invalid input should be rejected; the test still PASSES) ----------

    // Negative: correct username but wrong password -> rejected as invalid credentials.
    [Fact]
    public void Login_WithWrongPassword_ShouldFail()
    {
        var result = AuthService.TryLogin("secuser", "WrongPass1!", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.Equal(LoginFailureReason.InvalidCredentials, result.Reason);
        Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
    }

    // Negative: a username that does not exist -> rejected as invalid credentials.
    [Fact]
    public void Login_WithUnknownUsername_ShouldFail()
    {
        var result = AuthService.TryLogin("nouser1", "Sec123!a", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.Equal(LoginFailureReason.InvalidCredentials, result.Reason);
    }

    // Negative: empty username -> rejected because required fields are missing.
    [Fact]
    public void Login_WithEmptyUsername_ShouldFail()
    {
        var result = AuthService.TryLogin("", "Sec123!a", BuildEmployees());

        Assert.False(result.Success);
        Assert.Equal(LoginFailureReason.EmptyFields, result.Reason);
    }

    // Negative: empty password -> rejected because required fields are missing.
    [Fact]
    public void Login_WithEmptyPassword_ShouldFail()
    {
        var result = AuthService.TryLogin("secuser", "", BuildEmployees());

        Assert.False(result.Success);
        Assert.Equal(LoginFailureReason.EmptyFields, result.Reason);
    }

    // Positive: a freshly registered employee can log in immediately.
    [Fact]
    public void Login_ImmediatelyAfterRegistration_ShouldSucceed()
    {
        // Approval was removed: a freshly registered employee can log in right away.
        var employees = BuildEmployees();
        employees.Add(new Employee
        {
            Username = "newuser",
            Password = "New123!a",
            EmployeeNumber = "1009",
            Email = "new@clinic.com",
            IdNumber = "300000063",
            Role = "Secretary"
        });

        var result = AuthService.TryLogin("newuser", "New123!a", employees);

        Assert.True(result.Success);
        Assert.NotNull(result.Employee);
        Assert.Equal(LoginFailureReason.None, result.Reason);
    }

    // Positive: a successful login should report the Secretary role (used for screen access).
    [Fact]
    public void Login_ShouldReturnCorrectRole_ForSecretary()
    {
        var result = AuthService.TryLogin("secuser", "Sec123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.Equal("Secretary", result.Role);
    }

    // Positive: a successful login should report the Vet role (used for screen access).
    [Fact]
    public void Login_ShouldReturnCorrectRole_ForVeterinarian()
    {
        var result = AuthService.TryLogin("vetuser", "Vet123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.Equal("Vet", result.Role);
    }

    // Positive: valid credentials should succeed with no error message.
    [Fact]
    public void Login_WithValidCredentials_ShouldSucceed()
    {
        var result = AuthService.TryLogin("secuser", "Sec123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.NotNull(result.Employee);
        Assert.Equal(LoginFailureReason.None, result.Reason);
        Assert.True(string.IsNullOrEmpty(result.ErrorMessage));
    }

    // Negative: an email address is not a username, so it must be rejected.
    [Fact]
    public void Login_WithUnknownEmail_ShouldFail()
    {
        // Login uses Username, not email. An email that is not a registered username
        // must be rejected (you cannot sign in with an email address).
        var result = AuthService.TryLogin("ghost@clinic.com", "Sec123!a", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
    }

    // Negative: missing password -> rejected because required fields are missing.
    [Fact]
    public void Login_WithMissingPassword_ShouldFail()
    {
        var result = AuthService.TryLogin("secuser", "", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.Equal(LoginFailureReason.EmptyFields, result.Reason);
        Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
    }

    // Negative: an Employee ID is not a username, so it must be rejected.
    [Fact]
    public void Login_WithWrongEmployeeId_ShouldFail()
    {
        // Login uses Username, not Employee ID. Using an employee number as the
        // identifier must be rejected (you cannot sign in with an Employee ID).
        var result = AuthService.TryLogin("1002", "Sec123!a", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.False(string.IsNullOrEmpty(result.ErrorMessage));
    }
}
