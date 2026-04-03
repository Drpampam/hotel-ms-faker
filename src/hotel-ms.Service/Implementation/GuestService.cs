using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using Microsoft.EntityFrameworkCore;
using hotelier_core_app.Core.States;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Identity;

namespace hotelier_core_app.Service.Implementation
{
    public class GuestService : IGuestService
    {
        private readonly IDBCommandRepository<GuestProfile> _guestProfileCommandRepository;
        private readonly IDBQueryRepository<GuestProfile> _guestProfileQueryRepository;
        private readonly IDBQueryRepository<Reservation> _reservationQueryRepository;
        private readonly IDBQueryRepository<Room> _roomQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUtility _utility;

        public GuestService(
            IDBCommandRepository<GuestProfile> guestProfileCommandRepository,
            IDBQueryRepository<GuestProfile> guestProfileQueryRepository,
            IDBQueryRepository<Reservation> reservationQueryRepository,
            IDBQueryRepository<Room> roomQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            UserManager<ApplicationUser> userManager,
            IUtility utility)
        {
            _guestProfileCommandRepository = guestProfileCommandRepository;
            _guestProfileQueryRepository = guestProfileQueryRepository;
            _reservationQueryRepository = reservationQueryRepository;
            _roomQueryRepository = roomQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _userManager = userManager;
            _utility = utility;
        }

        public async Task<BaseResponse<GuestProfileResponseDTO>> CreateGuestProfileAsync(CreateGuestProfileRequestDTO request, AuditLog auditLog)
        {
            ApplicationUser? user = null;

            // If a platform user account is linked, validate it exists
            if (request.UserId.HasValue && request.UserId.Value != 0)
            {
                user = await _userManager.FindByIdAsync(request.UserId.Value.ToString());
                if (user == null)
                    return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);

                // Prevent duplicate profiles for the same user account
                var exists = await _guestProfileQueryRepository.IsExistAsync(g => g.UserId == request.UserId.Value && !g.IsDeleted);
                if (exists)
                    return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.GuestProfileAlreadyExists, ResponseStatusCode.GuestProfileAlreadyExists);
            }

            var profile = new GuestProfile
            {
                UserId = request.UserId,
                FullName = request.FullName,
                Email = request.Email,
                PhoneNumber = request.PhoneNumber,
                PassportNumber = request.PassportNumber,
                Nationality = request.Nationality,
                DateOfBirth = request.DateOfBirth,
                PreferredRoomType = request.PreferredRoomType,
                SpecialRequests = request.SpecialRequests,
                TenantId = request.TenantId,
                LoyaltyPoints = 0,
                LoyaltyTier = "Bronze",
                CreatedBy = auditLog.PerformedBy,
                CreationDate = DateTime.UtcNow,
            };

            _guestProfileCommandRepository.Add(profile);
            _auditLogCommandRepository.Add(auditLog);
            await _guestProfileCommandRepository.SaveAsync();

            var response = BuildGuestResponse(profile, user);
            return BaseResponse<GuestProfileResponseDTO>.Success(response, ResponseMessages.GuestProfileCreated, ResponseStatusCode.GuestProfileCreated);
        }

        public async Task<BaseResponse<GuestProfileResponseDTO>> UpdateGuestProfileAsync(UpdateGuestProfileRequestDTO request, AuditLog auditLog)
        {
            var profile = await _guestProfileQueryRepository.FindAsync(request.Id);
            if (profile == null)
                return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.GuestProfileNotFound, ResponseStatusCode.GuestProfileNotFound);

            if (request.PassportNumber != null) profile.PassportNumber = request.PassportNumber;
            if (request.Nationality != null) profile.Nationality = request.Nationality;
            if (request.DateOfBirth.HasValue) profile.DateOfBirth = request.DateOfBirth;
            if (request.PreferredRoomType != null) profile.PreferredRoomType = request.PreferredRoomType;
            if (request.SpecialRequests != null) profile.SpecialRequests = request.SpecialRequests;
            profile.ModifiedBy = auditLog.PerformedBy;
            profile.LastModifiedDate = DateTime.UtcNow;

            await _guestProfileCommandRepository.UpdateAsync(profile);
            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            ApplicationUser? user = null;
            if (profile.UserId.HasValue)
                user = await _userManager.FindByIdAsync(profile.UserId.Value.ToString());
            var response = BuildGuestResponse(profile, user);
            return BaseResponse<GuestProfileResponseDTO>.Success(response, ResponseMessages.GuestProfileUpdated, ResponseStatusCode.GuestProfileUpdated);
        }

        public async Task<BaseResponse<GuestProfileResponseDTO>> GetGuestProfileByIdAsync(long guestProfileId)
        {
            var profile = await _guestProfileQueryRepository.FindAsync(guestProfileId);
            if (profile == null)
                return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.GuestProfileNotFound, ResponseStatusCode.GuestProfileNotFound);

            ApplicationUser? user = null;
            if (profile.UserId.HasValue)
                user = await _userManager.FindByIdAsync(profile.UserId.Value.ToString());
            var response = BuildGuestResponse(profile, user);
            return BaseResponse<GuestProfileResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<GuestProfileResponseDTO>> GetGuestProfileByUserIdAsync(long userId)
        {
            var profile = await _guestProfileQueryRepository.GetByDefaultAsync(g => g.UserId == userId && !g.IsDeleted);
            if (profile == null)
                return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.GuestProfileNotFound, ResponseStatusCode.GuestProfileNotFound);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var response = BuildGuestResponse(profile, user);
            return BaseResponse<GuestProfileResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<GuestProfileResponseDTO>>> GetGuestsAsync(GetGuestsInputDTO input)
        {
            var all = await _guestProfileQueryRepository.GetByAsync(g =>
                !g.IsDeleted &&
                (!input.TenantId.HasValue || g.TenantId == input.TenantId.Value) &&
                (input.Nationality == null || g.Nationality == input.Nationality));

            var paginated = _utility.Paginate(all, input.PageNumber, input.PageSize).ToList();

            // Batch-load all linked users in ONE query instead of one per guest (N+1 fix)
            var linkedUserIds = paginated
                .Where(p => p.UserId.HasValue)
                .Select(p => p.UserId!.Value)
                .Distinct()
                .ToList();
            var users = linkedUserIds.Count > 0
                ? await _userManager.Users.Where(u => linkedUserIds.Contains(u.Id)).ToListAsync()
                : new List<ApplicationUser>();
            var userMap = users.ToDictionary(u => u.Id);

            var responses = new List<GuestProfileResponseDTO>();
            foreach (var profile in paginated)
            {
                ApplicationUser? user = null;
                if (profile.UserId.HasValue)
                    userMap.TryGetValue(profile.UserId.Value, out user);

                // Apply search term filter against name/email from profile or linked user
                if (input.SearchTerm != null)
                {
                    var term = input.SearchTerm.ToLower();
                    var name = (user?.FullName ?? profile.FullName ?? "").ToLower();
                    var email = (user?.Email ?? profile.Email ?? "").ToLower();
                    if (!name.Contains(term) && !email.Contains(term))
                        continue;
                }
                responses.Add(BuildGuestResponse(profile, user));
            }

            return PageBaseResponse<List<GuestProfileResponseDTO>>.Success(responses, ResponseMessages.GuestsRetrieved,
                count: responses.Count, totalPageCount: all.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }

        public async Task<PageBaseResponse<List<ReservationResponseDTO>>> GetGuestReservationsAsync(long guestProfileId, int pageNumber, int pageSize)
        {
            var profile = await _guestProfileQueryRepository.FindAsync(guestProfileId);
            if (profile == null)
                return PageBaseResponse<List<ReservationResponseDTO>>.Failure(new List<ReservationResponseDTO>(), ResponseMessages.GuestProfileNotFound);

            var reservations = await _reservationQueryRepository.GetByAsync(r => r.GuestId == profile.UserId && !r.IsDeleted);
            var paginated = _utility.Paginate(reservations, pageNumber, pageSize);
            ApplicationUser? user = null;
            if (profile.UserId.HasValue)
                user = await _userManager.FindByIdAsync(profile.UserId.Value.ToString());

            // Batch-load all required rooms in ONE query instead of one per reservation (N+1 fix)
            var paginatedList = paginated.ToList();
            var roomIds = paginatedList.Select(r => r.RoomId).Distinct().ToList();
            var rooms = await _roomQueryRepository.GetByAsync(r => roomIds.Contains(r.Id));
            var roomMap = rooms.ToDictionary(r => r.Id);

            var responses = new List<ReservationResponseDTO>();
            foreach (var r in paginatedList)
            {
                roomMap.TryGetValue(r.RoomId, out var room);
                int nights = (int)(r.CheckOutDate.Date - r.CheckInDate.Date).TotalDays;
                responses.Add(new ReservationResponseDTO
                {
                    Id = r.Id,
                    RoomId = r.RoomId,
                    RoomNumber = room?.Number,
                    RoomType = room?.Type,
                    GuestId = r.GuestId,
                    GuestName = user?.FullName,
                    GuestEmail = user?.Email,
                    CheckInDate = r.CheckInDate,
                    CheckOutDate = r.CheckOutDate,
                    NightsCount = nights,
                    TotalPrice = r.TotalPrice,
                    Status = r.Status,
                    SpecialRequests = r.SpecialRequests,
                    DiscountId = r.DiscountId,
                    CreatedBy = r.CreatedBy,
                    CreationDate = r.CreationDate,
                    LastModifiedDate = r.LastModifiedDate
                });
            }

            return PageBaseResponse<List<ReservationResponseDTO>>.Success(responses, ResponseMessages.ReservationsRetrieved,
                count: responses.Count, totalPageCount: reservations.Count(), pageSize: pageSize, pageNumber: pageNumber);
        }

        private GuestProfileResponseDTO BuildGuestResponse(GuestProfile profile, ApplicationUser? user) => new GuestProfileResponseDTO
        {
            Id = profile.Id,
            UserId = profile.UserId,
            FullName = user?.FullName ?? profile.FullName,
            Email = user?.Email ?? profile.Email,
            PhoneNumber = user?.PhoneNumber ?? profile.PhoneNumber,
            PassportNumber = profile.PassportNumber,
            Nationality = profile.Nationality,
            DateOfBirth = profile.DateOfBirth,
            PreferredRoomType = profile.PreferredRoomType,
            SpecialRequests = profile.SpecialRequests,
            LoyaltyPoints = profile.LoyaltyPoints,
            LoyaltyTier = profile.LoyaltyTier,
            TenantId = profile.TenantId,
            CreationDate = profile.CreationDate
        };
    }
}
