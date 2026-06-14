using System.Collections.Generic;
using System.Linq;
using ClinicVetsAvalonia.Data;
using ClinicVetsAvalonia.Models;

namespace ClinicVetsAvalonia.Services
{
    public enum LoginFailureReason
    {
        None,
        EmptyFields,
        InvalidUsernameFormat,
        InvalidCredentials,
        NotApproved
    }

    public sealed class LoginResult
    {
        public bool Success { get; init; }
        public string ErrorMessage { get; init; } = "";
        public Employee? Employee { get; init; }
        public string Role { get; init; } = "";
        public LoginFailureReason Reason { get; init; } = LoginFailureReason.None;

        public static LoginResult Fail(LoginFailureReason reason, string errorMessage) =>
            new() { Success = false, Reason = reason, ErrorMessage = errorMessage };

        public static LoginResult Ok(Employee employee) =>
            new() { Success = true, Employee = employee, Role = employee.Role };
    }

    public static class AuthService
    {
        public const string EmptyFieldsMessage = "יש למלא שם משתמש וסיסמה";
        public const string InvalidUsernameFormatMessage = "שם משתמש צריך להיות 6-8 תווים באנגלית, עד 2 ספרות";
        public const string InvalidCredentialsMessage = "שם משתמש או סיסמה שגויים";
        public const string NotApprovedMessage = "חשבון העובד טרם אושר";

        public static LoginResult TryLogin(string username, string password, IEnumerable<Employee> employees)
        {
            username = (username ?? "").Trim();
            password = (password ?? "").Trim();

            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return LoginResult.Fail(LoginFailureReason.EmptyFields, EmptyFieldsMessage);

            if (!ValidationService.IsValidUsername(username))
                return LoginResult.Fail(LoginFailureReason.InvalidUsernameFormat, InvalidUsernameFormatMessage);

            var employee = employees.FirstOrDefault(emp =>
                emp.Username == username && emp.Password == password);

            if (employee == null)
                return LoginResult.Fail(LoginFailureReason.InvalidCredentials, InvalidCredentialsMessage);

            if (!employee.IsApproved)
                return LoginResult.Fail(LoginFailureReason.NotApproved, NotApprovedMessage);

            return LoginResult.Ok(employee);
        }

        public static LoginResult TryLogin(string username, string password)
        {
            AppData.LoadEmployees();
            return TryLogin(username, password, AppData.Employees);
        }
    }
}
