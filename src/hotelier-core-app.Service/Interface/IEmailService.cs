using hotelier_core_app.Model.DTOs.Request;

namespace hotelier_core_app.Service.Interface
{
    public interface IEmailService : IAutoDependencyService
    {
        Task<bool> SendEmail(SendEmailDTO model);
    }
}
