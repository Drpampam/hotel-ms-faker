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
        private readonly IMapper _mapper;
        private readonly IUtility _utility;

        public GuestService(
            IDBCommandRepository<GuestProfile> guestProfileCommandRepository,
            IDBQueryRepository<GuestProfile> guestProfileQueryRepository,
            IDBQueryRepository<Reservation> reservationQueryRepository,
            IDBQueryRepository<Room> roomQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            UserManager<ApplicationUser> userManager,
            IMapper mapper,
            IUtility utility)
        {
            _guestProfileCommandRepository = guestProfileCommandRepository;
            _guestProfileQueryRepository = guestProfileQueryRepository;
            _reservationQueryRepository = reservationQueryRepository;
            _roomQueryRepository = roomQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _userManager = userManager;
            _mapper = mapper;
            _utility = utility;
        }

        public async Task<BaseResponse<GuestProfileResponseDTO>> CreateGuestProfileAsync(CreateGuestProfileRequestDTO request, AuditLog auditLog)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.UserDoesNotExist, ResponseStatusCode.UserDoesNotExist);

            var exists = await _guestProfileQueryRepository.IsExistAsync(g => g.UserId == request.UserId && !g.IsDeleted);
            if (exists)
                return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.GuestProfileAlreadyExists, ResponseStatusCode.GuestProfileAlreadyExists);

            var profile = _mapper.Map<GuestProfile>(request);
            profile.CreatedBy = auditLog.PerformedBy;
            profile.CreationDate = DateTime.UtcNow;
            profile.LoyaltyPoints = 0;
            profile.LoyaltyTier = "Bronze";

            _guestProfileCommandRepository.Add(profile);
            _auditLogCommandRepository.Add(auditLog);

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

            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
            var response = BuildGuestResponse(profile, user);
            return BaseResponse<GuestProfileResponseDTO>.Success(response, ResponseMessages.GuestProfileUpdated, ResponseStatusCode.GuestProfileUpdated);
        }

        public async Task<BaseResponse<GuestProfileResponseDTO>> GetGuestProfileByIdAsync(long guestProfileId)
        {
            var profile = await _guestProfileQueryRepository.FindAsync(guestProfileId);
            if (profile == null)
                return BaseResponse<GuestProfileResponseDTO>.Failure(new GuestProfileResponseDTO(), ResponseMessages.GuestProfileNotFound, ResponseStatusCode.GuestProfileNotFound);

            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
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

            var paginated = _utility.Paginate(all, input.PageNumber, input.PageSize);

            var responses = new List<GuestProfileResponseDTO>();
            foreach (var profile in paginated)
            {
                var user = await _userManager.FindByIdAsync(profile.UserId.ToString());
                // Apply search term filter on user name/email
                if (input.SearchTerm != null)
                {
                    var term = input.SearchTerm.ToLower();
                    if (user == null ||
                        (!(user.FullName?.ToLower().Contains(term) ?? false) &&
                         !(user.Email?.ToLower().Contains(term) ?? false)))
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
            var user = await _userManager.FindByIdAsync(profile.UserId.ToString());

            var responses = new List<ReservationResponseDTO>();
            foreach (var r in paginated)
            {
                var room = await _roomQueryRepository.FindAsync(r.RoomId);
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
            FullName = user?.FullName,
            Email = user?.Email,
            PhoneNumber = user?.PhoneNumber,
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
