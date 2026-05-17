using System.Net;
using System.Net.Mail;

namespace ClinicVets.Application.Services;

/// <summary>Sends password-reset codes via Gmail SMTP when configured; otherwise returns demo text.</summary>
public static class PasswordResetDelivery
{
    private const string GmailAddressEnvironmentVariable = "CLINIC_GMAIL_ADDRESS";
    private const string GmailPasswordEnvironmentVariable = "CLINIC_GMAIL_APP_PASSWORD";

    public static string GenerateCode() => Random.Shared.Next(100000, 999999).ToString();

    public static async Task<string> SendResetCodeAsync(string toEmail, string code)
    {
        var gmailAddress = Environment.GetEnvironmentVariable(GmailAddressEnvironmentVariable);
        var gmailAppPassword = Environment.GetEnvironmentVariable(GmailPasswordEnvironmentVariable);

        if (string.IsNullOrWhiteSpace(gmailAddress) || string.IsNullOrWhiteSpace(gmailAppPassword))
        {
            return $"Demo mode: Gmail is not configured on this PC. Your verification code is {code}";
        }

        using var message = new MailMessage(gmailAddress, toEmail)
        {
            Subject = "ClinicVets — password reset code",
            Body = $"Your ClinicVets password reset code is: {code}\nThe code is valid for 10 minutes.",
            IsBodyHtml = false
        };

        using var client = new SmtpClient("smtp.gmail.com", 587)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(gmailAddress, gmailAppPassword)
        };

        await client.SendMailAsync(message);
        return "A verification code was sent to the email you entered.";
    }
}
