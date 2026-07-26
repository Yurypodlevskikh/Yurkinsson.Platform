using Microsoft.AspNetCore.Identity.UI.Services;
using MailKit.Net.Smtp;
using MimeKit;
using YurkinssonAuthentication.DTOs;
using Microsoft.Extensions.Options;

namespace YurkinssonAuthentication.Services
{
    public class EmailService : IEmailSender
    {
        private readonly SmtpSettings _smtpSettings;

        public EmailService(IOptions<SmtpSettings> smtpSettings)
        {
            _smtpSettings = smtpSettings.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            var message = new MimeMessage();

            message.From.Add(new MailboxAddress("SpeedUp", _smtpSettings.From));
            message.To.Add(new MailboxAddress(email, email));
            message.Subject = subject;

            // Creation of the message body
            var bodyBuilder = new BodyBuilder
            {
                HtmlBody = htmlMessage
            };
            message.Body = bodyBuilder.ToMessageBody();

            // Sending the message
            using var smtpClient = new SmtpClient();
            try
            {
                await smtpClient.ConnectAsync(_smtpSettings.Smtp, _smtpSettings.Port, MailKit.Security.SecureSocketOptions.SslOnConnect);
                await smtpClient.AuthenticateAsync(_smtpSettings.From, _smtpSettings.Pass);
                await smtpClient.SendAsync(message);
            }
            finally
            {
                await smtpClient.DisconnectAsync(true);
            }
        }
    }
}
