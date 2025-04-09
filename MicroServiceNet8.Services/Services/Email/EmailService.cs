using MailKit.Net.Smtp;
using MicroServiceNet8.Services.Services.Email.Interfaces;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace AuthenNet8.Services.Services.Email
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task Send(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress("Smartlog Support", _config["Email:From"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            message.Body = new TextPart("html")
            {
                Text = body
            };

            using var client = new SmtpClient();
            if (!int.TryParse(_config["Email:SmtpPort"], out int port))
            {
                throw new InvalidOperationException("Invalid SMTP port configured.");
            }
            await client.ConnectAsync(_config["Email:SmtpHost"], port, true);
            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
