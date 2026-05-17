using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace ClinicVetsAvalonia.Services
{
    public static class PasswordResetService
    {
        private const string GmailAddressEnvironmentVariable = "CLINIC_GMAIL_ADDRESS";
        private const string GmailPasswordEnvironmentVariable = "CLINIC_GMAIL_APP_PASSWORD";

        public static string GenerateCode()
        {
            return Random.Shared.Next(100000, 999999).ToString();
        }

        public static async Task<string> SendResetCodeAsync(string toEmail, string code)
        {
            string? gmailAddress = Environment.GetEnvironmentVariable(GmailAddressEnvironmentVariable);
            string? gmailAppPassword = Environment.GetEnvironmentVariable(GmailPasswordEnvironmentVariable);

            if (string.IsNullOrWhiteSpace(gmailAddress) || string.IsNullOrWhiteSpace(gmailAppPassword))
            {
                return $"מצב דמו: לא הוגדר Gmail במחשב. קוד האימות הוא {code}";
            }

            using var message = new MailMessage(gmailAddress, toEmail)
            {
                Subject = "Clinic Vets - קוד איפוס סיסמה",
                Body = $"קוד איפוס הסיסמה שלך הוא: {code}\nהקוד תקף ל-10 דקות.",
                IsBodyHtml = false
            };

            using var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(gmailAddress, gmailAppPassword)
            };

            await client.SendMailAsync(message);

            return "קוד אימות נשלח לאימייל שהוזן";
        }
    }
}
