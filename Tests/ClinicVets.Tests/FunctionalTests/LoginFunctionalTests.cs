using System.Collections.Generic;
using ClinicVetsAvalonia.Models;
using ClinicVetsAvalonia.Services;

namespace ClinicVetsAvalonia.Tests;

public class LoginFunctionalTests
{
    private static List<Employee> BuildEmployees() =>
    [
        new Employee
        {
            Username = "secuser",
            Password = "Sec123!a",
            EmployeeNumber = "1002",
            Email = "sec@clinic.com",
            IdNumber = "300000027",
            Role = "Secretary",
            IsApproved = true
        },
        new Employee
        {
            Username = "vetuser",
            Password = "Vet123!a",
            EmployeeNumber = "1003",
            Email = "vet@clinic.com",
            IdNumber = "300000036",
            Role = "Vet",
            IsApproved = true
        },
        new Employee
        {
            Username = "penduser",
            Password = "Pass123!",
            EmployeeNumber = "2001",
            Email = "pending@clinic.com",
            IdNumber = "300000045",
            Role = "Secretary",
            IsApproved = false
        },
        new Employee
        {
            Username = "rejuser1",
            Password = "Pass123!",
            EmployeeNumber = "2002",
            Email = "rejected@clinic.com",
            IdNumber = "300000054",
            Role = "Vet",
            IsApproved = false
        }
    ];

    [Fact]
    public void Login_WithValidSecretaryCredentials_ShouldSucceed()
    {
        var result = AuthService.TryLogin("secuser", "Sec123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.NotNull(result.Employee);
        Assert.Equal("secuser", result.Employee!.Username);
        Assert.Equal(LoginFailureReason.None, result.Reason);
    }

    [Fact]
    public void Login_WithValidVetCredentials_ShouldSucceed()
    {
        var result = AuthService.TryLogin("vetuser", "Vet123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.NotNull(result.Employee);
        Assert.Equal("vetuser", result.Employee!.Username);
    }

    [Fact]
    public void Login_WithWrongPassword_ShouldFail()
    {
        var result = AuthService.TryLogin("secuser", "WrongPass1!", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.Equal(LoginFailureReason.InvalidCredentials, result.Reason);
    }

    [Fact]
    public void Login_WithUnknownUsername_ShouldFail()
    {
        var result = AuthService.TryLogin("nouser1", "Sec123!a", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.Equal(LoginFailureReason.InvalidCredentials, result.Reason);
    }

    [Fact]
    public void Login_WithEmptyUsername_ShouldFail()
    {
        var result = AuthService.TryLogin("", "Sec123!a", BuildEmployees());

        Assert.False(result.Success);
        Assert.Equal(LoginFailureReason.EmptyFields, result.Reason);
    }

    [Fact]
    public void Login_WithEmptyPassword_ShouldFail()
    {
        var result = AuthService.TryLogin("secuser", "", BuildEmployees());

        Assert.False(result.Success);
        Assert.Equal(LoginFailureReason.EmptyFields, result.Reason);
    }

    [Fact]
    public void Login_WithRejectedEmployee_ShouldFail()
    {
        var result = AuthService.TryLogin("rejuser1", "Pass123!", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.Equal(LoginFailureReason.NotApproved, result.Reason);
    }

    [Fact]
    public void Login_WithPendingEmployee_ShouldFail()
    {
        var result = AuthService.TryLogin("penduser", "Pass123!", BuildEmployees());

        Assert.False(result.Success);
        Assert.Null(result.Employee);
        Assert.Equal(LoginFailureReason.NotApproved, result.Reason);
    }

    [Fact]
    public void Login_ShouldReturnCorrectRole_ForSecretary()
    {
        var result = AuthService.TryLogin("secuser", "Sec123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.Equal("Secretary", result.Role);
    }

    [Fact]
    public void Login_ShouldReturnCorrectRole_ForVeterinarian()
    {
        var result = AuthService.TryLogin("vetuser", "Vet123!a", BuildEmployees());

        Assert.True(result.Success);
        Assert.Equal("Vet", result.Role);
    }
}
