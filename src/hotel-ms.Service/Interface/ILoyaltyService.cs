using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface ILoyaltyService : IAutoDependencyService
    {
        Task<BaseResponse<LoyaltyResponseDTO>> GetLoyaltyByUserIdAsync(long userId);
        Task<BaseResponse<LoyaltyResponseDTO>> AccruePointsAsync(long userId, int points, string reason);
        Task<BaseResponse<LoyaltyResponseDTO>> RedeemPointsAsync(long userId, int points, long reservationId, AuditLog auditLog);
    }
}
