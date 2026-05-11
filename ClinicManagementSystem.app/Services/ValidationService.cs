using System.Linq;

namespace ClinicManagementSystem.app.Services
{
    public static class ValidationService
    {
        public static bool IsValidUsername(string username)
        {
            return username.Length >= 6 && username.Length <= 8;
        }

        public static bool IsValidPassword(string password)
        {
            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => !char.IsLetterOrDigit(ch));

            return password.Length >= 8 &&
                   password.Length <= 10 &&
                   hasLetter &&
                   hasDigit &&
                   hasSpecial;
        }

        public static bool IsValidEmployeeNumber(string employeeNumber)
        {
            return employeeNumber.Length == 4 && employeeNumber.All(char.IsDigit);
        }

        public static bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }
    }
}