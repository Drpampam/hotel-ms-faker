using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IRoomService : IAutoDependencyService
    {
        // CRUD
        Task<BaseResponse<RoomResponseDTO>> AddRoomAsync(AddRoomRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<RoomResponseDTO>> UpdateRoomAsync(UpdateRoomRequestDTO request, AuditLog auditLog);
        Task<BaseResponse<RoomResponseDTO>> GetRoomByIdAsync(long roomId);
        Task<PageBaseResponse<List<RoomResponseDTO>>> GetRoomsAsync(GetRoomsInputDTO input);
        Task<BaseResponse> DeleteRoomAsync(long roomId, AuditLog auditLog);

        // State machine
        Task<BaseResponse<RoomStateResponseDTO>> ChangeRoomStateAsync(long roomId, RoomTrigger trigger);
        Task<BaseResponse<RoomStateResponseDTO>> GetRoomStateAsync(long roomId);
        Task<BaseResponse<List<RoomTrigger>>> GetAvailableTriggersAsync(long roomId);
    }
}
