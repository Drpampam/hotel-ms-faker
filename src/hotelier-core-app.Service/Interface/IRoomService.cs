using hotelier_core_app.Core.States;
using hotelier_core_app.Model.DTOs.Response;

namespace hotelier_core_app.Service.Interface
{
    public interface IRoomService : IAutoDependencyService
    {
        Task<BaseResponse<RoomStateResponseDTO>> ChangeRoomStateAsync(long roomId, RoomTrigger trigger);
        Task<BaseResponse<RoomStateResponseDTO>> GetRoomStateAsync(long roomId);
        Task<BaseResponse<List<RoomTrigger>>> GetAvailableTriggersAsync(long roomId);
    }
}
