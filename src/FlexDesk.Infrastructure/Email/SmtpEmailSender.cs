using System.Net.Mail;
using FlexDesk.Application.Common.Email;
using Microsoft.Extensions.Options;

namespace FlexDesk.Infrastructure.Email;

public class SmtpEmailSender : IEmailSender
{
    private readonly SmtpSettings _settings;

    public SmtpEmailSender(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient(_settings.Host, _settings.Port);
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromAddress, _settings.FromName),
            Subject = subject,
            Body = body
        };
        message.To.Add(to);

        await client.SendMailAsync(message, cancellationToken);
    }
}
