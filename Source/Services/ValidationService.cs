using System;
using System.Linq;

namespace ClinicVetsAvalonia.Services
{
    public static class ValidationService
    {
        // ---------- Employee Validation ----------

        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < 6 || username.Length > 8)
                return false;

            int digitCount = username.Count(char.IsDigit);
            bool onlyEnglishLettersOrDigits = username.All(ch =>
                (ch >= 'A' && ch <= 'Z') ||
                (ch >= 'a' && ch <= 'z') ||
                char.IsDigit(ch));

            return digitCount <= 2 && onlyEnglishLettersOrDigits;
        }

        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            bool hasLetter = password.Any(char.IsLetter);
            bool hasDigit = password.Any(char.IsDigit);
            bool hasSpecial = password.Any(ch => ch == '!' || ch == '$' || ch == '#' || ch == ',');

            return password.Length >= 8 &&
                   password.Length <= 10 &&
                   hasLetter &&
                   hasDigit &&
                   hasSpecial;
        }

        public static bool IsValidEmployeeNumber(string employeeNumber)
        {
            return !string.IsNullOrWhiteSpace(employeeNumber) &&
                   employeeNumber.Length == 4 &&
                   employeeNumber.All(char.IsDigit);
        }

        public static bool IsValidRole(string role)
        {
            return role == "Secretary" || role == "Vet";
        }

        // ---------- Client / General Validation ----------

        public static bool IsValidFullName(string fullName)
        {
            return !string.IsNullOrWhiteSpace(fullName) &&
                   fullName.All(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch));
        }

        public static bool IsValidIdNumber(string idNumber)
        {
            return !string.IsNullOrWhiteSpace(idNumber) &&
                   idNumber.Length == 9 &&
                   idNumber.All(char.IsDigit);
        }

        public static bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone) &&
                   phone.Length >= 9 &&
                   phone.Length <= 10 &&
                   phone.All(char.IsDigit);
        }

        public static bool IsValidEmail(string email)
        {
            return GetEmailValidationMessage(email) == null;
        }

        public static string? GetEmailValidationMessage(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return "יש להזין אימייל";

            email = email.Trim();

            if (!email.Contains('@'))
                return "אימייל חייב לכלול @";

            int atIndex = email.IndexOf('@');
            if (atIndex <= 0)
                return "אימייל חייב לכלול @";

            if (email.LastIndexOf('@') != atIndex)
                return "אימייל חייב לכלול @ אחד בלבד";

            string domain = email[(atIndex + 1)..];
            if (string.IsNullOrWhiteSpace(domain))
                return "חסר דומיין אחרי @";

            if (!domain.Contains('.'))
                return "אימייל חייב להסתיים ב-.com, .co.il, .net, .org או .il";

            string[] allowedSuffixes = { ".co.il", ".com", ".net", ".org", ".il" };
            bool hasValidSuffix = allowedSuffixes.Any(suffix =>
                domain.EndsWith(suffix, StringComparison.OrdinalIgnoreCase));

            if (!hasValidSuffix)
                return "אימייל חייב להסתיים ב-.com, .co.il, .net, .org או .il";

            return null;
        }

        public static string? GetBirthDateValidationMessage(DateTime birthDate)
        {
            if (birthDate.Date > DateTime.Today)
                return "תאריך לידה חייב להיות בעבר";

            if (birthDate.Year < 2000)
                return "תאריך לידה לא יכול להיות לפני שנת 2000";

            return null;
        }

        // ---------- Animal Validation ----------

        public static bool IsValidAnimalName(string name)
        {
            return !string.IsNullOrWhiteSpace(name) &&
                   name.All(ch => char.IsLetter(ch) || char.IsWhiteSpace(ch));
        }

        public static bool IsValidAnimalSpecies(string species)
        {
            return species == "Dog" ||
                   species == "Cat" ||
                   species == "Reptile" ||
                   species == "Bird" ||
                   species == "כלב" ||
                   species == "חתול" ||
                   species == "זוחל" ||
                   species == "ציפור";
        }

        public static bool IsValidWeight(double weight)
        {
            return weight >= 0.1 && weight <= 100;
        }

        public static bool IsValidBirthDate(DateTime birthDate)
        {
            return birthDate.Year >= 2000 &&
                   birthDate.Date <= DateTime.Today;
        }

        public static bool IsValidChipNumber(string chipNumber)
        {
            return !string.IsNullOrWhiteSpace(chipNumber) &&
                   chipNumber.Length == 7 &&
                   chipNumber.StartsWith("376", StringComparison.Ordinal) &&
                   chipNumber.All(char.IsDigit);
        }

        public static bool IsValidVaccinationDate(DateTime vaccinationDate)
        {
            return vaccinationDate.Date <= DateTime.Today;
        }

        public static bool IsValidVaccinationDateForBirthDate(DateTime vaccinationDate, DateTime birthDate)
        {
            return vaccinationDate.Date >= birthDate.Date &&
                   IsValidVaccinationDate(vaccinationDate);
        }

        // ---------- Visit / Medication Validation ----------

        public static bool IsRequiredText(string value)
        {
            return !string.IsNullOrWhiteSpace(value);
        }

        public static bool IsValidVisitDate(DateTime visitDate)
        {
            return visitDate.Year >= 2000;
        }

        public static bool IsValidMoney(double value)
        {
            return value >= 0;
        }

        public static bool IsValidStockQuantity(int quantity)
        {
            return quantity >= 0;
        }

        public static bool IsValidMedicationQuantity(int quantity)
        {
            return quantity >= 0;
        }

        public static bool IsValidExpirationDate(DateTime expirationDate)
        {
            return expirationDate.Date >= DateTime.Today;
        }

        public static bool IsVaccinationDue(DateTime lastVaccinationDate)
        {
            return lastVaccinationDate.Date <= DateTime.Today.AddYears(-1);
        }
    }
}