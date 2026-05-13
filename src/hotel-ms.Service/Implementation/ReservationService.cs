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
        private readonly IDBQueryRepository<ReservationExpense> _expenseQueryRepository;
        private readonly INotificationService _notificationService;
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
            IDBQueryRepository<ReservationExpense> expenseQueryRepository,
            INotificationService notificationService,
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
            _expenseQueryRepository = expenseQueryRepository;
            _notificationService = notificationService;
            _mapper = mapper;
            _utility = utility;
            _context = context;
            _logger = logger;
        }

        // Returns the GuestProfile.Id for the given caller email, or null if not found.
        private async Task<long?> ResolveGuestProfileIdByEmail(string email)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;
            var profile = await _context.GuestProfiles.FirstOrDefaultAsync(g => g.UserId == user.Id && !g.IsDeleted);
            return profile?.Id;
        }

        public async Task<BaseResponse<ReservationResponseDTO>> CreateReservationAsync(CreateReservationRequestDTO request, AuditLog auditLog, string? callerEmail = null)
        {
            // Npgsql requires DateTimeKind.Utc for timestamptz columns — date-only strings arrive as Unspecified
            request.CheckInDate = DateTime.SpecifyKind(request.CheckInDate, DateTimeKind.Utc);
            request.CheckOutDate = DateTime.SpecifyKind(request.CheckOutDate, DateTimeKind.Utc);

            // Guest-role callers may only book for themselves — override any supplied guestId
            if (!string.IsNullOrEmpty(callerEmail))
            {
                var ownProfileId = await ResolveGuestProfileIdByEmail(callerEmail);
                if (ownProfileId == null)
                    return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), "No guest profile found for your account. Please contact the front desk.", ResponseStatusCode.UserDoesNotExist);
                request.GuestId = ownProfileId.Value;
            }

            _logger.LogInformation("Creating reservation for room {RoomId}, guest {GuestId}, dates {CheckIn}-{CheckOut}",
                request.RoomId, request.GuestId, request.CheckInDate, request.CheckOutDate);

            if (request.CheckOutDate <= request.CheckInDate)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.InvalidDateRange, ResponseStatusCode.InvalidData);

            var guest = await _context.GuestProfiles.FindAsync(request.GuestId);
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

            // Notify the hotel company about the new booking
            _ = _notificationService.SendNewBookingAlertAsync(
                reservation,
                guest?.FullName ?? "Guest",
                guest?.Email ?? "N/A",
                room.Number ?? room.Id.ToString());

            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationCreated, ResponseStatusCode.ReservationCreated);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> UpdateReservationAsync(UpdateReservationRequestDTO request, AuditLog auditLog)
        {
            if (request.CheckInDate.HasValue)
                request.CheckInDate = DateTime.SpecifyKind(request.CheckInDate.Value, DateTimeKind.Utc);
            if (request.CheckOutDate.HasValue)
                request.CheckOutDate = DateTime.SpecifyKind(request.CheckOutDate.Value, DateTimeKind.Utc);

            var reservation = await _reservationQueryRepository.FindAsync(request.Id);
            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            if (reservation.Status == ReservationState.Cancelled || reservation.Status == ReservationState.CheckedOut)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotCancellable, ResponseStatusCode.InvalidData);

            // Once checked in, dates and room are locked — use admin override if correction needed
            if (reservation.Status == ReservationState.CheckedIn &&
                (request.CheckInDate.HasValue || request.CheckOutDate.HasValue || request.RoomId.HasValue))
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationDatesLocked, ResponseStatusCode.InvalidData);

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
            await _auditLogCommandRepository.SaveAsync();

            var guest = await _context.GuestProfiles.FindAsync(reservation.GuestId);
            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationUpdated, ResponseStatusCode.ReservationUpdated);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> GetReservationByIdAsync(long reservationId, string? callerEmail = null)
        {
            var reservation = _reservationQueryRepository.FindByInclude(
                r => r.Id == reservationId,
                r => r.Room,
                r => r.GuestProfile).FirstOrDefault();

            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            if (!string.IsNullOrEmpty(callerEmail))
            {
                var ownProfileId = await ResolveGuestProfileIdByEmail(callerEmail);
                if (ownProfileId == null || reservation.GuestId != ownProfileId.Value)
                    return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);
            }

            var expenses = await _expenseQueryRepository.GetByAsync(e => e.ReservationId == reservationId && !e.IsDeleted);
            var response = BuildReservationResponse(reservation, reservation.Room, reservation.GuestProfile, expenses.ToList());
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<ReservationResponseDTO>>> GetReservationsAsync(GetReservationsInputDTO input, string? callerEmail = null)
        {
            // Guest-role callers may only see their own reservations
            if (!string.IsNullOrEmpty(callerEmail))
            {
                var ownProfileId = await ResolveGuestProfileIdByEmail(callerEmail);
                input.GuestId = ownProfileId ?? -1; // -1 returns nothing if no profile found
            }

            // Use direct context query with Include so Room and User navigation properties are populated.
            // The generic repository's GetByAsync does not eager-load navigation properties,
            // causing r.Room and r.GuestProfile to be null and producing empty reservation records.
            var query = _context.Reservations
                .Include(r => r.Room)
                .Include(r => r.GuestProfile)
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

            var reservationIds = paginated.Select(r => r.Id).ToList();
            var allExpenses = reservationIds.Any()
                ? (await _expenseQueryRepository.GetByAsync(e => reservationIds.Contains(e.ReservationId) && !e.IsDeleted)).ToList()
                : new List<ReservationExpense>();

            var responses = paginated.Select(r =>
                BuildReservationResponse(r, r.Room, r.GuestProfile, allExpenses.Where(e => e.ReservationId == r.Id).ToList())
            ).ToList();

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
            await _auditLogCommandRepository.SaveAsync();

            var room = await _roomQueryRepository.FindAsync(reservation.RoomId);
            var guest = await _context.GuestProfiles.FindAsync(reservation.GuestId);
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

            if (DateTime.UtcNow.Date > reservation.CheckOutDate.Date)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationExpired, ResponseStatusCode.InvalidData);

            reservation.Status = ReservationState.CheckedIn;
            reservation.ModifiedBy = auditLog.PerformedBy;
            reservation.LastModifiedDate = DateTime.UtcNow;
            reservation.ActualCheckInDate = DateTime.UtcNow;

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
            await _auditLogCommandRepository.SaveAsync();

            var guest = await _context.GuestProfiles.FindAsync(reservation.GuestId);

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
            reservation.ActualCheckOutDate = DateTime.UtcNow;

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
            await _context.SaveChangesAsync();

            var guest = await _context.GuestProfiles.FindAsync(reservation.GuestId);

            if (!string.IsNullOrEmpty(guest?.Email))
                _ = _notificationService.SendCheckOutSummaryAsync(reservation, guest.Email, guest.FullName ?? guest.Email);

            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.ReservationCheckedOut, ResponseStatusCode.CheckedOut);
        }

        public async Task<BaseResponse<ReservationResponseDTO>> OverrideStatusAsync(long reservationId, string status, AuditLog auditLog)
        {
            if (!Enum.TryParse<ReservationState>(status, ignoreCase: true, out var newStatus))
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), $"Invalid status '{status}'", ResponseStatusCode.InvalidData);

            var reservation = await _reservationQueryRepository.FindAsync(reservationId);
            if (reservation == null)
                return BaseResponse<ReservationResponseDTO>.Failure(new ReservationResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            reservation.Status = newStatus;
            reservation.ModifiedBy = auditLog.PerformedBy;
            reservation.LastModifiedDate = DateTime.UtcNow;

            await _reservationCommandRepository.UpdateAsync(reservation);
            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            var room = await _roomQueryRepository.FindAsync(reservation.RoomId);
            var guest = await _context.GuestProfiles.FindAsync(reservation.GuestId);
            var response = BuildReservationResponse(reservation, room, guest);
            return BaseResponse<ReservationResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        private ReservationResponseDTO BuildReservationResponse(Reservation reservation, Room? room, GuestProfile? guest, List<ReservationExpense>? expenses = null)
        {
            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;
            var expenseList = expenses ?? new List<ReservationExpense>();
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
                ExpensesTotal = expenseList.Sum(e => e.Amount),
                Status = reservation.Status,
                SpecialRequests = reservation.SpecialRequests,
                DiscountId = reservation.DiscountId,
                CreatedBy = reservation.CreatedBy,
                CreationDate = reservation.CreationDate,
                LastModifiedDate = reservation.LastModifiedDate,
                Expenses = expenseList.Select(e => new ReservationExpenseResponseDTO
                {
                    Id = e.Id,
                    ReservationId = e.ReservationId,
                    Description = e.Description,
                    Category = e.Category,
                    Quantity = e.Quantity,
                    UnitPrice = e.UnitPrice,
                    Amount = e.Amount,
                    CreatedBy = e.CreatedBy,
                    CreationDate = e.CreationDate
                }).ToList()
            };
        }
    }
}
