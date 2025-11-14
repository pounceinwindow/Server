// HttpServer.Services/EmailService.cs

using System.Net;
using System.Net.Mail;

namespace HttpServer.Services;

public static class EmailService
{
    public static async Task SendAsync(string to, string subject, string htmlBody)
    {
        var cfg = EmailConfig.Instance;
        using var message = new MailMessage(
            new MailAddress(cfg.FromAddr, cfg.FromName),
            new MailAddress(to))
        {
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };

        using var smtp = new SmtpClient(cfg.SmtpHost, cfg.SmtpPort)
        {
            EnableSsl = true,
            Credentials = new NetworkCredential(cfg.SmtpUser, cfg.SmtpPass)
        };

        await smtp.SendMailAsync(message);
    }
}