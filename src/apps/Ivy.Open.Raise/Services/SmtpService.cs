using MailKit.Net.Smtp;
using MimeKit;

namespace Ivy.Open.Raise.Services;

public interface ISmtpService
{
    Task SendAsync(MimeMessage mail);
}


public class SmtpService(string host, int port, string user, string password) : ISmtpService
{
    public async Task SendAsync(MimeMessage mail)
    {
        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(user, password);
        await client.SendAsync(mail);
        await client.DisconnectAsync(true);
    }
}

public static class SmtpServiceExtensions
{
    public static void UseSmtp(this IServiceCollection services)
    {
        services.AddSingleton<ISmtpService>(s =>
        {
            var configuration = s.GetRequiredService<IConfiguration>();
            var smtpHost = configuration["Smtp:Host"];
            var smtpPort = int.Parse(configuration["Smtp:Port"]);
            var smtpUser = configuration["Smtp:User"];
            var smtpPassword = configuration["Smtp:Password"];
            return new SmtpService(smtpHost, smtpPort, smtpUser, smtpPassword);
        });
    }
}

public class SmtpSecrets : IHaveSecrets
{
    public Secret[] GetSecrets()
    {
        return
        [
            new("Smtp:Host"),
            new("Smtp:Port"),
            new("Smtp:User"),
            new("Smtp:Password")
        ];
    }
}