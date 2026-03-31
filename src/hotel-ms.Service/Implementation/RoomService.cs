using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Core.States;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
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
        private readonly IDBQueryRepository<Property> _propertyQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;

        public RoomService(
            IDBCommandRepository<Room> roomCommandRepository,
            IDBQueryRepository<Room> roomQueryRepository,
            IDBQueryRepository<Property> propertyQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            IMapper mapper,
            IUtility utility)
        {
            _roomCommandRepository = roomCommandRepository;
            _roomQueryRepository = roomQueryRepository;
            _propertyQueryRepository = propertyQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _mapper = mapper;
            _utility = utility;
        }

        public async Task<BaseResponse<RoomResponseDTO>> AddRoomAsync(AddRoomRequestDTO request, AuditLog auditLog)
        {
            var property = await _propertyQueryRepository.FindAsync(request.PropertyId);
            if (property == null)
                return BaseResponse<RoomResponseDTO>.Failure(new RoomResponseDTO(), ResponseMessages.PropertyNotFound, ResponseStatusCode.NoRecordFound);

            var room = _mapper.Map<Room>(request);
            room.IsAvailable = true;
            room.RoomState = RoomState.Available;
            room.CreatedBy = auditLog.PerformedBy;
            room.CreationDate = DateTime.UtcNow;

            _roomCommandRepository.Add(room);
            _auditLogCommandRepository.Add(auditLog);

            var response = _mapper.Map<RoomResponseDTO>(room);
            return BaseResponse<RoomResponseDTO>.Success(response, ResponseMessages.RoomCreated, ResponseStatusCode.RoomCreated);
        }

        public async Task<BaseResponse<RoomResponseDTO>> UpdateRoomAsync(UpdateRoomRequestDTO request, AuditLog auditLog)
        {
            var room = await _roomQueryRepository.FindAsync(request.Id);
            if (room == null)
                return BaseResponse<RoomResponseDTO>.Failure(new RoomResponseDTO(), ResponseMessages.RoomNotFound, ResponseStatusCode.RoomNotFound);

            if (request.Number != null) room.Number = request.Number;
            if (request.Type != null) room.Type = request.Type;
            if (request.Capacity.HasValue) room.Capacity = request.Capacity.Value;
            if (request.PricePerNight.HasValue) room.PricePerNight = request.PricePerNight.Value;
            room.ModifiedBy = auditLog.PerformedBy;
            room.LastModifiedDate = DateTime.UtcNow;

            await _roomCommandRepository.UpdateAsync(room);
            _auditLogCommandRepository.Add(auditLog);

            var response = _mapper.Map<RoomResponseDTO>(room);
            return BaseResponse<RoomResponseDTO>.Success(response, ResponseMessages.RoomUpdated, ResponseStatusCode.RoomUpdated);
        }

        public async Task<BaseResponse<RoomResponseDTO>> GetRoomByIdAsync(long roomId)
        {
            var room = await _roomQueryRepository.FindAsync(roomId);
            if (room == null)
                return BaseResponse<RoomResponseDTO>.Failure(new RoomResponseDTO(), ResponseMessages.RoomNotFound, ResponseStatusCode.RoomNotFound);

            var response = _mapper.Map<RoomResponseDTO>(room);
            return BaseResponse<RoomResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<RoomResponseDTO>>> GetRoomsAsync(GetRoomsInputDTO input)
        {
            var allRooms = await _roomQueryRepository.GetByAsync(r =>
                !r.IsDeleted &&
                (!input.PropertyId.HasValue || r.PropertyId == input.PropertyId.Value) &&
                (input.Type == null || r.Type == input.Type) &&
                (!input.IsAvailable.HasValue || r.IsAvailable == input.IsAvailable.Value) &&
                (!input.MaxPrice.HasValue || r.PricePerNight <= input.MaxPrice.Value));

            var paginated = _utility.Paginate(allRooms, input.PageNumber, input.PageSize);
            var response = _mapper.Map<List<RoomResponseDTO>>(paginated);

            return PageBaseResponse<List<RoomResponseDTO>>.Success(response, ResponseMessages.RoomsRetrieved,
                count: response.Count, totalPageCount: allRooms.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }

        public async Task<BaseResponse> DeleteRoomAsync(long roomId, AuditLog auditLog)
        {
            var room = await _roomQueryRepository.FindAsync(roomId);
            if (room == null)
                return BaseResponse.Failure(ResponseMessages.RoomNotFound, ResponseStatusCode.RoomNotFound);

            room.IsDeleted = true;
            room.ModifiedBy = auditLog.PerformedBy;
            room.LastModifiedDate = DateTime.UtcNow;

            await _roomCommandRepository.UpdateAsync(room);
            _auditLogCommandRepository.Add(auditLog);

            return BaseResponse.Success(ResponseMessages.RoomDeleted, ResponseStatusCode.RoomDeleted);
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
