using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IDiscountService : IAutoDependencyService
    {
        Task<BaseResponse<DiscountResponseDTO>> CreateDiscountAsync(CreateDiscountRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<DiscountResponseDTO>> UpdateDiscountAsync(UpdateDiscountRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<DiscountResponseDTO>> GetDiscountByIdAsync(long discountId);
        Task<PageBaseResponse<List<DiscountResponseDTO>>> GetDiscountsAsync(GetDiscountsInputDTO input);
        Task<BaseResponse> DeleteDiscountAsync(long discountId, AuditLog auditLog);
    }
}
