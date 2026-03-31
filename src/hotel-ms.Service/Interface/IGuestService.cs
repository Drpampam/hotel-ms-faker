using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IGuestService : IAutoDependencyService
    {
        Task<BaseResponse<GuestProfileResponseDTO>> CreateGuestProfileAsync(CreateGuestProfileRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<GuestProfileResponseDTO>> UpdateGuestProfileAsync(UpdateGuestProfileRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<GuestProfileResponseDTO>> GetGuestProfileByIdAsync(long guestProfileId);
        Task<BaseResponse<GuestProfileResponseDTO>> GetGuestProfileByUserIdAsync(long userId);
        Task<PageBaseResponse<List<GuestProfileResponseDTO>>> GetGuestsAsync(GetGuestsInputDTO input);
        Task<PageBaseResponse<List<ReservationResponseDTO>>> GetGuestReservationsAsync(long guestProfileId, int pageNumber, int pageSize);
    }
}
