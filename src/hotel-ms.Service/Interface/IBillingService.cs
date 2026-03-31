using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IBillingService : IAutoDependencyService
    {
        Task<BaseResponse<InvoiceResponseDTO>> GenerateInvoiceAsync(long reservationId, AuditLog auditLog);
        Task<BaseResponse<InvoiceResponseDTO>> GetInvoiceByIdAsync(long invoiceId);
        Task<BaseResponse<InvoiceResponseDTO>> GetInvoiceByReservationIdAsync(long reservationId);
        Task<PageBaseResponse<List<InvoiceResponseDTO>>> GetInvoicesAsync(GetInvoicesInputDTO input);
        Task<BaseResponse<InvoiceResponseDTO>> MarkInvoicePaidAsync(long invoiceId, AuditLog auditLog);
        Task<BaseResponse<InvoiceResponseDTO>> VoidInvoiceAsync(long invoiceId, AuditLog auditLog);
    }
}
