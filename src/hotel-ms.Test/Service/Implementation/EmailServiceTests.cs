using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Service.Implementation;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class EmailServiceTests
    {
        private static IConfiguration BuildConfig() =>
            new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["EmailSettings:Host"] = "smtp.example.com",
                    ["EmailSettings:Port"] = "587",
                    ["EmailSettings:EnableSsl"] = "true",
                    ["EmailSettings:Username"] = "user@example.com",
                    ["EmailSettings:Password"] = "password",
                    ["EmailSettings:FromAddress"] = "noreply@example.com",
                    ["EmailSettings:FromName"] = "Test"
                })
                .Build();

        [Fact]
        public async Task SendEmail_ShouldReturnFalse_WhenSmtpFails()
        {
            var service = new EmailService(BuildConfig());
            var dto = new SendEmailDTO(new List<string> { "test@example.com" }, "Subject", "Body");
            var result = await service.SendEmail(dto);
            Assert.False(result);
        }

        [Fact]
        public async Task SendEmail_ShouldReturnFalse_WhenRecipientsEmpty()
        {
            var service = new EmailService(BuildConfig());
            var dto = new SendEmailDTO(new List<string>(), "Subject", "Body");
            var result = await service.SendEmail(dto);
            Assert.False(result);
        }
    }
}
