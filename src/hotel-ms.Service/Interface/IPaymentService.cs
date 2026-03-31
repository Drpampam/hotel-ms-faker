using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IPaymentService : IAutoDependencyService
    {
        // CRUD
        Task<BaseResponse<PaymentResponseDTO>> CreatePaymentAsync(CreatePaymentRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<PaymentResponseDTO>> GetPaymentByIdAsync(long paymentId);
        Task<PageBaseResponse<List<PaymentResponseDTO>>> GetPaymentsAsync(GetPaymentsInputDTO input);

        // State machine
        Task<BaseResponse<PaymentStateResponseDTO>> ChangePaymentStateAsync(long paymentId, PaymentTrigger trigger);
        Task<BaseResponse<PaymentStateResponseDTO>> GetPaymentStateAsync(long paymentId);
        Task<BaseResponse<List<PaymentTrigger>>> GetAvailableTriggersAsync(long paymentId);
    }
}
