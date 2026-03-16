using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.States;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation
{
    /// <summary>
    /// Provides business logic for managing rooms and their state transitions.
    /// </summary>
    public class RoomService : IRoomService
    {
        private readonly IDBCommandRepository<Room> _roomCommandRepository;
        private readonly IDBQueryRepository<Room> _roomQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IMapper _mapper;

        public RoomService(
            IDBCommandRepository<Room> roomCommandRepository,
            IDBQueryRepository<Room> roomQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IMapper mapper)
        {
            _roomCommandRepository = roomCommandRepository;
            _roomQueryRepository = roomQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// Retrieves the current state and available triggers for a room.
        /// </summary>
        /// <param name="roomId">The ID of the room.</param>
        /// <returns>Returns the room state and available triggers, or failure if not found.</returns>
        public async Task<BaseResponse<RoomStateResponseDTO>> GetRoomStateAsync(long roomId)
        {
            var room = await _roomQueryRepository.FindAsync(roomId);
            if (room == null)
                return BaseResponse<RoomStateResponseDTO>.Failure(new RoomStateResponseDTO(), "Room not found", ResponseStatusCode.NoRecordFound);

            room.ConfigureStateMachine();
            var triggers = room.StateMachine != null ? (await room.StateMachine.PermittedTriggersAsync).ToList() : new List<RoomTrigger>();

            var responseDto = new RoomStateResponseDTO
            {
                RoomId = room.Id,
                State = room.RoomState,
                AvailableTriggers = triggers
            };

            return BaseResponse<RoomStateResponseDTO>.Success(responseDto, "State fetched successfully", ResponseStatusCode.OperationSuccessful);
        }

        /// <summary>
        /// Changes the state of a room using the specified trigger.
        /// </summary>
        /// <param name="roomId">The ID of the room.</param>
        /// <param name="trigger">The trigger to fire for the state change.</param>
        /// <returns>Returns the updated room state, or failure if not found or trigger is invalid.</returns>
        public async Task<BaseResponse<RoomStateResponseDTO>> ChangeRoomStateAsync(long roomId, RoomTrigger trigger)
        {
            var room = await _roomQueryRepository.FindAsync(roomId);
            if (room == null)
                return BaseResponse<RoomStateResponseDTO>.Failure(new RoomStateResponseDTO(), "Room not found", ResponseStatusCode.NoRecordFound);

            room.ConfigureStateMachine();
            if (room.StateMachine == null || !room.StateMachine.CanFire(trigger))
                return BaseResponse<RoomStateResponseDTO>.Failure(new RoomStateResponseDTO(), "Invalid trigger", ResponseStatusCode.InvalidData);

            room.StateMachine.Fire(trigger);
            await _roomCommandRepository.UpdateAsync(room);

            var triggers = room.StateMachine != null ? (await room.StateMachine.PermittedTriggersAsync).ToList() : new List<RoomTrigger>();

            var responseDto = new RoomStateResponseDTO
            {
                RoomId = room.Id,
                State = room.RoomState,
                AvailableTriggers = triggers
            };

            return BaseResponse<RoomStateResponseDTO>.Success(responseDto, "State changed successfully", ResponseStatusCode.OperationSuccessful);
        }

        /// <summary>
        /// Retrieves all available triggers for a room's current state.
        /// </summary>
        /// <param name="roomId">The ID of the room.</param>
        /// <returns>Returns a list of available triggers, or failure if room not found.</returns>
        public async Task<BaseResponse<List<RoomTrigger>>> GetAvailableTriggersAsync(long roomId)
        {
            var room = await _roomQueryRepository.FindAsync(roomId);
            if (room == null)
                return BaseResponse<List<RoomTrigger>>.Failure(new List<RoomTrigger>(), "Room not found", ResponseStatusCode.NoRecordFound);

            room.ConfigureStateMachine();
            var triggers = room.StateMachine != null ? (await room.StateMachine.PermittedTriggersAsync).ToList() : new List<RoomTrigger>();

            return BaseResponse<List<RoomTrigger>>.Success(triggers, "Available triggers fetched successfully", ResponseStatusCode.OperationSuccessful);
        }
    }
}
