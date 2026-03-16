using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Response;

namespace hotelier_core_app.Service.Interface
{
    public interface IPaymentService : IAutoDependencyService
    {
        Task<BaseResponse<PaymentStateResponseDTO>> ChangePaymentStateAsync(long paymentId, PaymentTrigger trigger);
        Task<BaseResponse<PaymentStateResponseDTO>> GetPaymentStateAsync(long paymentId);
        Task<BaseResponse<List<PaymentTrigger>>> GetAvailableTriggersAsync(long paymentId);
    }
}
