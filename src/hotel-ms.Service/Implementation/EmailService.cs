using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation
{
    /// <summary>
    /// Provides email sending functionality for the application.
    /// </summary>
    public class EmailService : IEmailService
    {
        /// <summary>
        /// Sends an email using the provided email details.
        /// </summary>
        /// <param name="model">The email details including recipients, subject, and body.</param>
        /// <returns>Returns a task that resolves to true if the email was sent successfully, otherwise false.</returns>
        public Task<bool> SendEmail(SendEmailDTO model)
        {
            throw new NotImplementedException();
        }
    }
}
