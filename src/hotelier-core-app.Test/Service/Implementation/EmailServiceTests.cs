using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Service.Implementation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class EmailServiceTests
    {
        private readonly EmailService _service = new EmailService();

        [Fact]
        public async Task SendEmail_ShouldThrowNotImplementedException()
        {
            var dto = new SendEmailDTO(new List<string> { "test@example.com" }, "Test Subject", "Test Message");
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.SendEmail(dto));
        }

        [Fact]
        public async Task SendEmail_ShouldThrowNotImplementedException_WhenDtoIsNull()
        {
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.SendEmail(null));
        }

        [Fact]
        public async Task SendEmail_ShouldThrowNotImplementedException_WhenRecipientsEmpty()
        {
            var dto = new SendEmailDTO(new List<string>(), "Test Subject", "Test Message");
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.SendEmail(dto));
        }

        [Fact]
        public async Task SendEmail_ShouldThrowNotImplementedException_WhenRecipientsInvalidEmail()
        {
            var dto = new SendEmailDTO(new List<string> { "invalid-email" }, "Test Subject", "Test Message");
            await Assert.ThrowsAsync<NotImplementedException>(() => _service.SendEmail(dto));
        }
    }
}
