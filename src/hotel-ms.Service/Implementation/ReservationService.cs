using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Core.States;
using hotelier_core_app.Migrations;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace hotelier_core_app.Service.Implementation
{
    public class ReservationService : IReservationService
    {
        private readonly IDBCommandRepository<Reservation> _reservationCommandRepository;
        private readonly IDBQueryRepository<Reservation> _reservationQueryRepository;
        private readonly IDBQueryRepository<Room> _roomQueryRepository;
        private readonly IDBCommandRepository<Room> _roomCommandRepository;
        private readonly IDBQueryRepository<Discount> _discountQueryRepository;
        private readonly IDBCommandRepository<HousekeepingTask> _housekeepingTaskCommandRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly INotificationService _notificationService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IUtility _utility;
        private readonly AppDbContext _context;
        private readonly ILogger<ReservationService> _logger;

        public ReservationService(
            IDBCommandRepository<Reservation> reservationCommandRepository,
            IDBQueryRepository<Reservation> reservationQueryRepository,
            IDBQueryRepository<Room> roomQueryRepository,
            IDBCommandRepository<Room> roomCommandRepository,
            IDBQueryRepository<Discount> discountQueryRepository,
            IDBCommandRepository<HousekeepingTask> housekeepingTaskCommandRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            INotificationService notificationService,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUtility utility,
            AppDbContext context,
            ILogger<ReservationService> logger)
        {
            _reservationCommandRepository = reservationCommandRepository;
            _reservationQueryRepository = reservationQueryRepository;
            _roomQueryRepository = roomQueryRepository;
            _roomCommandRepository = roomCommandRepository;
            _discountQueryRepository = discountQueryRepository;
            _housekeepingTaskCommandRepository = housekeepingTaskCommandRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _notificationService = notificationService;
            _userManager = userManager;
            _mapper = mapper;
            _utility = utility;
            _context = context;
            _logger = logger;
        }

        public async Task<BaseResponse<ReservationResponseDTO>> CreateReservationAsync(CreateReservationRequestDTO request, AuditLog auditLog)
        {
            _logger.LogInformation("Creating reservation for room {RoomId}, guest {GuestId}, dates {CheckIn}-{CheckOut}",
                request.RoomId, request.GuestId, request.CheckInDate, request.CheckOutDate);

            if (request.CheckOutDate <= request.CheckInDate)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.InvalidDateRange, ResponseStatusCode.InvalidData);

            var guest = await _userManager.FindByIdAsync(request.GuestId.ToString());
            if (guest == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);

            var room = await _roomQueryRepository.FindAsync(request.RoomId);
            if (room == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.RoomNotFound, ResponseStatusCode.RoomNotFound);

            int nights = (int)(request.CheckOutDate.Date - request.CheckInDate.Date).TotalDays;
            decimal totalPrice = nights * room.PricePerNight;

            // Apply discount if provided
            if (request.DiscountId.HasValue)
            {
                var discount = await _discountQueryRepository.FindAsync(request.DiscountId.Value);
                if (discount != null && discount.IsActive && !discount.IsDeleted)
                {
                    if (discount.EndDate.HasValue && discount.EndDate.Value < DateTime.UtcNow)
                        return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.DiscountExpired, ResponseStatusCode.InvalidData);

                    if (discount.Percentage > 0)
                        totalPrice -= totalPrice * (discount.Percentage / 100);
                    else if (discount.FixedAmount.HasValue)
                        totalPrice -= discount.FixedAmount.Value;

                    if (totalPrice < 0) totalPrice = 0;
                }
            }

            var reservation = new Reservation
            {
                RoomId = request.RoomId,
                GuestId = request.GuestId,
                CheckInDate = request.CheckInDate,
                CheckOutDate = request.CheckOutDate,
                TotalPrice = totalPrice,
                DiscountId = request.DiscountId,
                SpecialRequests = request.SpecialRequests,
                Status = ReservationState.Confirmed,
                CreatedBy = auditLog.PerformedBy,
                CreationDate = DateTime.UtcNow
            };

            using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);
            try
            {
                // Atomic availability check within the serializable transaction
                var conflicting = await _reservationQueryRepository.GetByAsync(r =>
                    r.RoomId == request.RoomId &&
                    !r.IsDeleted &&
                    r.Status != ReservationState.Cancelled &&
                    r.Status != ReservationState.CheckedOut &&
                    r.CheckInDate < request.CheckOutDate &&
                    r.CheckOutDate > request.CheckInDate);

                if (conflicting.Any())
                {
                    _logger.LogWarning("Room {RoomId} is not available for dates {CheckIn}-{CheckOut}",
                        request.RoomId, request.CheckInDate, request.CheckOutDate);
                    await transaction.RollbackAsync();
                    return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.RoomNotAvailable, ResponseStatusCode.RoomNotAvailable);
                }

                _reservationCommandRepository.Add(reservation);
                await _context.SaveChangesAsync();

                _auditLogCommandRepository.Add(auditLog);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex) when (ex.Message.Contains("23P01") || ex.Message.Contains("duplicate") || ex.Message.Contains("overlap"))
            {
                await transaction.RollbackAsync();
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.RoomNotAvailable, ResponseStatusCode.RoomNotAvailable);
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            if (!string.IsNullOrEmpty(guest?.Email))
                _ = _notificationService.SendReservationConfirmedAsync(reservation, guest.Email, guest.FullName ?? guest.Email);

            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationCreated, ResponseStatusCode.ReservationCreated);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> UpdateReservationAsync(UpdateReservationRequestDTO request, AuditLog auditLog)
        {
            var reservation = await _reservationQueryRepository.FindAsync(request.Id);
            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            if (reservation.Status == ReservationState.Cancelled || reservation.Status == ReservationState.CheckedOut)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotCancellable, ResponseStatusCode.InvalidData);

            var newCheckIn = request.CheckInDate ?? reservation.CheckInDate;
            var newCheckOut = request.CheckOutDate ?? reservation.CheckOutDate;

            if (newCheckOut <= newCheckIn)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.InvalidDateRange, ResponseStatusCode.InvalidData);

            long roomId = request.RoomId ?? reservation.RoomId;

            // If room or dates changed, re-check availability
            if (request.RoomId.HasValue || request.CheckInDate.HasValue || request.CheckOutDate.HasValue)
            {
                var conflicting = await _reservationQueryRepository.GetByAsync(r =>
                    r.RoomId == roomId &&
                    r.Id != reservation.Id &&
                    !r.IsDeleted &&
                    r.Status != ReservationState.Cancelled &&
                    r.Status != ReservationState.CheckedOut &&
                    r.CheckInDate < newCheckOut &&
                    r.CheckOutDate > newCheckIn);

                if (conflicting.Any())
                    return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.RoomNotAvailable, ResponseStatusCode.RoomNotAvailable);
            }

            var room = await _roomQueryRepository.FindAsync(roomId);
            if (room == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.RoomNotFound, ResponseStatusCode.RoomNotFound);

            reservation.RoomId = roomId;
            reservation.CheckInDate = newCheckIn;
            reservation.CheckOutDate = newCheckOut;
            reservation.SpecialRequests = request.SpecialRequests ?? reservation.SpecialRequests;
            reservation.DiscountId = request.DiscountId ?? reservation.DiscountId;
            reservation.ModifiedBy = auditLog.PerformedBy;
            reservation.LastModifiedDate = DateTime.UtcNow;

            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;
            reservation.TotalPrice = nights * room.PricePerNight;

            await _reservationCommandRepository.UpdateAsync(reservation);
            _auditLogCommandRepository.Add(auditLog);

            var guest = await _userManager.FindByIdAsync(reservation.GuestId.ToString());
            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationUpdated, ResponseStatusCode.ReservationUpdated);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> GetReservationByIdAsync(long reservationId)
        {
            var reservation = _reservationQueryRepository.FindByInclude(
                r => r.Id == reservationId,
                r => r.Room,
                r => r.User).FirstOrDefault();

            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            var response = BuildReservationResponse(reservation, reservation.Room, reservation.User);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<ReservationResponseDTO>>> GetReservationsAsync(GetReservationsInputDTO input)
        {
            // Use direct context query with Include so Room and User navigation properties are populated.
            // The generic repository's GetByAsync does not eager-load navigation properties,
            // causing r.Room and r.User to be null and producing empty reservation records.
            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.User)
                .Where(r =>
                    !r.IsDeleted &&
                    (!input.GuestId.HasValue || r.GuestId == input.GuestId.Value) &&
                    (!input.RoomId.HasValue || r.RoomId == input.RoomId.Value) &&
                    (!input.Status.HasValue || r.Status == input.Status.Value) &&
                    (!input.FromDate.HasValue || r.CheckInDate >= input.FromDate.Value) &&
                    (!input.ToDate.HasValue || r.CheckOutDate <= input.ToDate.Value));

            var totalCount = await query.CountAsync();
            var paginated = await query
                .OrderByDescending(r => r.CreationDate)
                .Skip((input.PageNumber - 1) * input.PageSize)
                .Take(input.PageSize)
                .ToListAsync();

            var responses = paginated.Select(r => BuildReservationResponse(r, r.Room, r.User)).ToList();

            return PageBaseResponse<List<ReservationResponseDTO>>.Success(responses, ResponseMessages.ReservationsRetrieved,
                count: responses.Count, totalPageCount: totalCount, pageSize: input.PageSize, pageNumber: input.PageNumber);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> CancelReservationAsync(long reservationId, AuditLog auditLog)
        {
            var reservation = await _reservationQueryRepository.FindAsync(reservationId);
            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            if (reservation.Status != ReservationState.Pending && reservation.Status != ReservationState.Confirmed)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotCancellable, ResponseStatusCode.InvalidData);

            reservation.Status = ReservationState.Cancelled;
            reservation.ModifiedBy = auditLog.PerformedBy;
            reservation.LastModifiedDate = DateTime.UtcNow;

            await _reservationCommandRepository.UpdateAsync(reservation);
            _auditLogCommandRepository.Add(auditLog);

            var room = await _roomQueryRepository.FindAsync(reservation.RoomId);
            var guest = await _userManager.FindByIdAsync(reservation.GuestId.ToString());
            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationCancelled, ResponseStatusCode.ReservationCancelled);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> CheckInAsync(long reservationId, AuditLog auditLog)
        {
            _logger.LogInformation("Reservation {ReservationId} checked in", reservationId);

            var reservation = await _reservationQueryRepository.FindAsync(reservationId);
            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            if (reservation.Status != ReservationState.Confirmed)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotCheckInable, ResponseStatusCode.InvalidData);

            reservation.Status = ReservationState.CheckedIn;
            reservation.ModifiedBy = auditLog.PerformedBy;
            reservation.LastModifiedDate = DateTime.UtcNow;

            await _reservationCommandRepository.UpdateAsync(reservation);

            // Fire room state machine → Occupied
            var room = await _roomQueryRepository.FindAsync(reservation.RoomId);
            if (room != null)
            {
                room.ConfigureStateMachine();
                if (room.StateMachine != null && room.StateMachine.CanFire(RoomTrigger.CheckIn))
                {
                    room.StateMachine.Fire(RoomTrigger.CheckIn);
                    room.IsAvailable = false;
                    await _roomCommandRepository.UpdateAsync(room);
                }
            }

            _auditLogCommandRepository.Add(auditLog);

            var guest = await _userManager.FindByIdAsync(reservation.GuestId.ToString());

            if (!string.IsNullOrEmpty(guest?.Email))
                _ = _notificationService.SendCheckInWelcomeAsync(reservation, guest.Email, guest.FullName ?? guest.Email);

            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationCheckedIn, ResponseStatusCode.CheckedIn);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> CheckOutAsync(long reservationId, AuditLog auditLog)
        {
            _logger.LogInformation("Reservation {ReservationId} checked out", reservationId);

            var reservation = await _reservationQueryRepository.FindAsync(reservationId);
            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            if (reservation.Status != ReservationState.CheckedIn)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotCheckOutable, ResponseStatusCode.InvalidData);

            reservation.Status = ReservationState.CheckedOut;
            reservation.ModifiedBy = auditLog.PerformedBy;
            reservation.LastModifiedDate = DateTime.UtcNow;

            await _reservationCommandRepository.UpdateAsync(reservation);

            // Fire room state machine → Cleaning
            var room = await _roomQueryRepository.FindAsync(reservation.RoomId);
            if (room != null)
            {
                room.ConfigureStateMachine();
                if (room.StateMachine != null && room.StateMachine.CanFire(RoomTrigger.SetCleaning))
                {
                    room.StateMachine.Fire(RoomTrigger.SetCleaning);
                    room.IsAvailable = false;
                    await _roomCommandRepository.UpdateAsync(room);
                }
            }

            // Auto-create a housekeeping cleaning task for the checked-out room
            if (room != null)
            {
                var cleaningTask = new HousekeepingTask
                {
                    RoomId = room.Id,
                    TaskType = "Cleaning",
                    Priority = "High",
                    Notes = $"Auto-created on checkout for reservation #{reservation.Id}",
                    ScheduledAt = DateTime.UtcNow,
                    State = HousekeepingTaskState.Pending,
                    CreatedBy = "System",
                    CreationDate = DateTime.UtcNow
                };
                _housekeepingTaskCommandRepository.Add(cleaningTask);
            }

            _auditLogCommandRepository.Add(auditLog);

            var guest = await _userManager.FindByIdAsync(reservation.GuestId.ToString());

            if (!string.IsNullOrEmpty(guest?.Email))
                _ = _notificationService.SendCheckOutSummaryAsync(reservation, guest.Email, guest.FullName ?? guest.Email);

            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationCheckedOut, ResponseStatusCode.CheckedOut);
        }

        private ReservationResponseDTO BuildReservationResponse(Reservation reservation, Room? room, ApplicationUser? guest)
        {
            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;
            return new ReservationResponseDTO
            {
                Id = reservation.Id,
                RoomId = reservation.RoomId,
                RoomNumber = room?.Number,
                RoomType = room?.Type,
                GuestId = reservation.GuestId,
                GuestName = guest?.FullName,
                GuestEmail = guest?.Email,
                CheckInDate = reservation.CheckInDate,
                CheckOutDate = reservation.CheckOutDate,
                NightsCount = nights,
                TotalPrice = reservation.TotalPrice,
                Status = reservation.Status,
                SpecialRequests = reservation.SpecialRequests,
                DiscountId = reservation.DiscountId,
                CreatedBy = reservation.CreatedBy,
                CreationDate = reservation.CreationDate,
                LastModifiedDate = reservation.LastModifiedDate
            };
        }
    }
}
