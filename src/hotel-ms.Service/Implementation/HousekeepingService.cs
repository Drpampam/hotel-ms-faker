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
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace hotelier_core_app.Service.Implementation
{
    public class HousekeepingService : IHousekeepingService
    {
        private readonly IDBCommandRepository<HousekeepingTask> _taskCommandRepository;
        private readonly IDBQueryRepository<HousekeepingTask> _taskQueryRepository;
        private readonly IDBQueryRepository<Room> _roomQueryRepository;
        private readonly IDBCommandRepository<Room> _roomCommandRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;
        private readonly ILogger<HousekeepingService> _logger;

        public HousekeepingService(
            IDBCommandRepository<HousekeepingTask> taskCommandRepository,
            IDBQueryRepository<HousekeepingTask> taskQueryRepository,
            IDBQueryRepository<Room> roomQueryRepository,
            IDBCommandRepository<Room> roomCommandRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUtility utility,
            ILogger<HousekeepingService> logger)
        {
            _taskCommandRepository = taskCommandRepository;
            _taskQueryRepository = taskQueryRepository;
            _roomQueryRepository = roomQueryRepository;
            _roomCommandRepository = roomCommandRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _userManager = userManager;
            _mapper = mapper;
            _utility = utility;
            _logger = logger;
        }

        public async Task<BaseResponse<HousekeepingTaskResponseDTO>> CreateTaskAsync(CreateHousekeepingTaskDTO request, AuditLog auditLog)
        {
            _logger.LogInformation("Creating housekeeping task for room {RoomId}, type {TaskType}", request.RoomId, request.TaskType);

            var room = await _roomQueryRepository.FindAsync(request.RoomId);
            if (room == null)
                return BaseResponse<HousekeepingTaskResponseDTO>.Failure(new HousekeepingTaskResponseDTO(), ResponseMessages.RoomNotFound, ResponseStatusCode.RoomNotFound);

            // Normalize ScheduledAt to UTC (datetime-local inputs arrive as Kind=Unspecified)
            if (request.ScheduledAt.HasValue)
                request.ScheduledAt = DateTime.SpecifyKind(request.ScheduledAt.Value, DateTimeKind.Utc);

            var task = new HousekeepingTask
            {
                RoomId = request.RoomId,
                AssignedToUserId = request.AssignedToUserId,
                TaskType = request.TaskType,
                Priority = request.Priority,
                Notes = request.Notes,
                ScheduledAt = request.ScheduledAt ?? DateTime.UtcNow,
                TenantId = request.TenantId,
                State = HousekeepingTaskState.Pending,
                CreatedBy = auditLog.PerformedBy,
                CreationDate = DateTime.UtcNow
            };

            _taskCommandRepository.Add(task);
            _auditLogCommandRepository.Add(auditLog);
            await _taskCommandRepository.SaveAsync();

            var response = await BuildTaskResponse(task, room);
            return BaseResponse<HousekeepingTaskResponseDTO>.Success(response, ResponseMessages.HousekeepingTaskCreated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<HousekeepingTaskResponseDTO>> GetTaskByIdAsync(long taskId)
        {
            var task = await _taskQueryRepository.FindAsync(taskId);
            if (task == null)
                return BaseResponse<HousekeepingTaskResponseDTO>.Failure(new HousekeepingTaskResponseDTO(), ResponseMessages.HousekeepingTaskNotFound, ResponseStatusCode.NoRecordFound);

            var room = await _roomQueryRepository.FindAsync(task.RoomId);
            var response = await BuildTaskResponse(task, room);
            return BaseResponse<HousekeepingTaskResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<HousekeepingTaskResponseDTO>>> GetTasksAsync(GetHousekeepingTasksInputDTO input)
        {
            var all = await _taskQueryRepository.GetByAsync(t =>
                !t.IsDeleted &&
                (!input.RoomId.HasValue || t.RoomId == input.RoomId.Value) &&
                (!input.AssignedToUserId.HasValue || t.AssignedToUserId == input.AssignedToUserId.Value) &&
                (input.TaskType == null || t.TaskType == input.TaskType) &&
                (!input.State.HasValue || t.State == input.State.Value) &&
                (!input.TenantId.HasValue || t.TenantId == input.TenantId.Value) &&
                (!input.ScheduledDate.HasValue || (t.ScheduledAt.HasValue && t.ScheduledAt.Value.Date == input.ScheduledDate.Value.Date)));

            var paginated = _utility.Paginate(all, input.PageNumber, input.PageSize);
            var responses = new List<HousekeepingTaskResponseDTO>();
            foreach (var task in paginated)
            {
                var room = await _roomQueryRepository.FindAsync(task.RoomId);
                responses.Add(await BuildTaskResponse(task, room));
            }

            return PageBaseResponse<List<HousekeepingTaskResponseDTO>>.Success(responses, ResponseMessages.OperationSuccessful,
                count: responses.Count, totalPageCount: all.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }

        public async Task<BaseResponse<HousekeepingTaskResponseDTO>> ChangeTaskStateAsync(long taskId, HousekeepingTaskTrigger trigger, AuditLog auditLog)
        {
            _logger.LogInformation("Task {TaskId} state changed via trigger {Trigger}", taskId, trigger);

            var task = await _taskQueryRepository.FindAsync(taskId);
            if (task == null)
                return BaseResponse<HousekeepingTaskResponseDTO>.Failure(new HousekeepingTaskResponseDTO(), ResponseMessages.HousekeepingTaskNotFound, ResponseStatusCode.NoRecordFound);

            task.ConfigureStateMachine();
            if (task.StateMachine == null || !task.StateMachine.CanFire(trigger))
                return BaseResponse<HousekeepingTaskResponseDTO>.Failure(new HousekeepingTaskResponseDTO(), ResponseMessages.InvalidData, ResponseStatusCode.InvalidData);

            task.StateMachine.Fire(trigger);
            task.ModifiedBy = auditLog.PerformedBy;
            task.LastModifiedDate = DateTime.UtcNow;

            if (trigger == HousekeepingTaskTrigger.Complete)
                task.CompletedAt = DateTime.UtcNow;

            await _taskCommandRepository.UpdateAsync(task);
            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            var room = await _roomQueryRepository.FindAsync(task.RoomId);

            // If task completes a Cleaning, mark room Available again
            if (trigger == HousekeepingTaskTrigger.Complete && task.TaskType == "Cleaning" && room != null)
            {
                room.ConfigureStateMachine();
                if (room.StateMachine != null && room.StateMachine.CanFire(RoomTrigger.FinishCleaning))
                {
                    room.StateMachine.Fire(RoomTrigger.FinishCleaning);
                    room.IsAvailable = true;
                    await _roomCommandRepository.UpdateAsync(room);
                }
            }

            var response = await BuildTaskResponse(task, room);
            return BaseResponse<HousekeepingTaskResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<List<HousekeepingTaskResponseDTO>>> GetDailyScheduleAsync(long tenantId, DateTime date)
        {
            var tasks = await _taskQueryRepository.GetByAsync(t =>
                !t.IsDeleted &&
                t.TenantId == tenantId &&
                t.ScheduledAt.HasValue &&
                t.ScheduledAt.Value.Date == date.Date);

            var responses = new List<HousekeepingTaskResponseDTO>();
            foreach (var task in tasks)
            {
                var room = await _roomQueryRepository.FindAsync(task.RoomId);
                responses.Add(await BuildTaskResponse(task, room));
            }

            return BaseResponse<List<HousekeepingTaskResponseDTO>>.Success(responses, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        private async Task<HousekeepingTaskResponseDTO> BuildTaskResponse(HousekeepingTask task, Room? room)
        {
            ApplicationUser? assignedTo = null;
            if (task.AssignedToUserId.HasValue)
                assignedTo = await _userManager.FindByIdAsync(task.AssignedToUserId.Value.ToString());

            task.ConfigureStateMachine();
            var triggers = task.StateMachine != null
                ? (await task.StateMachine.PermittedTriggersAsync).ToList()
                : new List<HousekeepingTaskTrigger>();

            return new HousekeepingTaskResponseDTO
            {
                Id = task.Id,
                RoomId = task.RoomId,
                RoomNumber = room?.Number,
                AssignedToUserId = task.AssignedToUserId,
                AssignedToName = assignedTo?.FullName,
                TaskType = task.TaskType,
                Priority = task.Priority,
                Notes = task.Notes,
                State = task.State,
                AvailableTriggers = triggers,
                ScheduledAt = task.ScheduledAt,
                CompletedAt = task.CompletedAt,
                TenantId = task.TenantId,
                CreatedBy = task.CreatedBy,
                CreationDate = task.CreationDate
            };
        }

    }
}
