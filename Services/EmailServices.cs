// 17/01/2026 - 22:32:30
// DANGTHUY

using LushEnglishAPI.Models;

namespace LushEnglishAPI.Services;

using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

public class EmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string to, string subject, string html)
    {
        var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = html,
            IsBodyHtml = true
        };

        mail.To.Add(to);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.UseSsl,
            Credentials = new NetworkCredential(
                _settings.Username,
                _settings.Password
            )
        };

        await client.SendMailAsync(mail);
    }
}
