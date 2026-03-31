using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IReservationService : IAutoDependencyService
    {
        Task<BaseResponse<ReservationResponseDTO>> CreateReservationAsync(CreateReservationRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<ReservationResponseDTO>> UpdateReservationAsync(UpdateReservationRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<ReservationResponseDTO>> GetReservationByIdAsync(long reservationId);
        Task<PageBaseResponse<List<ReservationResponseDTO>>> GetReservationsAsync(GetReservationsInputDTO input);
        Task<BaseResponse<ReservationResponseDTO>> CancelReservationAsync(long reservationId, AuditLog auditLog);
        Task<BaseResponse<ReservationResponseDTO>> CheckInAsync(long reservationId, AuditLog auditLog);
        Task<BaseResponse<ReservationResponseDTO>> CheckOutAsync(long reservationId, AuditLog auditLog);
    }
}
