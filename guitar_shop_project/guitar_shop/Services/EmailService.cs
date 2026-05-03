using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;

namespace guitar_shop.Services;

public interface IEmailService
{
    Task SendConfirmationEmailAsync(string email, string username, string token);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendConfirmationEmailAsync(string email, string username, string token)
    {
        var smtpSettings = _config.GetSection("SmtpSettings");
        var mockMode = smtpSettings.GetValue<bool>("MockMode");

        if (mockMode)
        {
            // В режиме mock просто логируем "отправку" письма
            _logger.LogInformation($"[MOCK EMAIL] Письмо подтверждения для {email} ({username}). Токен: {token}");
            _logger.LogInformation($"[MOCK EMAIL] Ссылка для подтверждения: /Auth/Confirm?token={token}&email={email}");
            return;
        }

        var fromEmail = smtpSettings.GetValue<string>("FromEmail")!;
        var host = smtpSettings.GetValue<string>("Host")!;
        var port = smtpSettings.GetValue<int>("Port");
        var enableSsl = smtpSettings.GetValue<bool>("EnableSsl");
        var smtpUsername = smtpSettings.GetValue<string>("Username");
        var smtpPassword = smtpSettings.GetValue<string>("Password");

        var confirmationLink = $"/Auth/Confirm?token={token}&email={email}";
        
        using var message = new MailMessage(fromEmail, email);
        message.Subject = "Подтверждение регистрации в Guitar Shop";
        message.Body = $@"
            Здравствуйте, {username}!
            
            Спасибо за регистрацию в нашем магазине.
            
            Пожалуйста, подтвердите ваш email, перейдя по ссылке:
            {confirmationLink}
            
            Если вы не регистрировались, просто проигнорируйте это письмо.
        ";
        message.IsBodyHtml = false;

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl
        };

        if (!string.IsNullOrEmpty(smtpUsername))
        {
            client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
        }

        await client.SendMailAsync(message);
        _logger.LogInformation($"Письмо подтверждения отправлено на {email}");
    }
}
