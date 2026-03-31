using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Service.Interface;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;

namespace hotelier_core_app.Service.Implementation
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<bool> SendEmail(SendEmailDTO model)
        {
            try
            {
                var host = _configuration["EmailSettings:Host"]!;
                var port = _configuration.GetValue<int>("EmailSettings:Port");
                var enableSsl = _configuration.GetValue<bool>("EmailSettings:EnableSsl");
                var username = _configuration["EmailSettings:Username"]!;
                var password = _configuration["EmailSettings:Password"]!;
                var fromAddress = _configuration["EmailSettings:FromAddress"]!;
                var fromName = _configuration["EmailSettings:FromName"] ?? "Hotelier";

                using var client = new SmtpClient(host, port)
                {
                    Credentials = new NetworkCredential(username, password),
                    EnableSsl = enableSsl
                };

                var message = new MailMessage
                {
                    From = new MailAddress(fromAddress, fromName),
                    Subject = model.Subject,
                    Body = model.Message,
                    IsBodyHtml = true
                };

                foreach (var recipient in model.Recipient ?? Enumerable.Empty<string>())
                    message.To.Add(recipient);

                foreach (var cc in model.Cc ?? Enumerable.Empty<string>())
                    message.CC.Add(cc);

                if (model.Attachment != null)
                {
                    foreach (var (fileName, stream) in model.Attachment)
                        message.Attachments.Add(new Attachment(stream, fileName));
                }

                await client.SendMailAsync(message);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
