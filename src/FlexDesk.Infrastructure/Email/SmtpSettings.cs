namespace FlexDesk.Infrastructure.Email;

public class SmtpSettings
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 1025;
    public string FromAddress { get; set; } = "no-reply@flexdesk.local";
    public string FromName { get; set; } = "FlexDesk";
}
