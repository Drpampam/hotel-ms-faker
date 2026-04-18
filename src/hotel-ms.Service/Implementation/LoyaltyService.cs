using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace hotelier_core_app.Service.Implementation
{
    public class LoyaltyService : ILoyaltyService
    {
        private readonly IDBCommandRepository<LoyaltyProgram> _loyaltyCommandRepository;
        private readonly IDBQueryRepository<LoyaltyProgram> _loyaltyQueryRepository;
        private readonly IDBCommandRepository<GuestProfile> _guestProfileCommandRepository;
        private readonly IDBQueryRepository<GuestProfile> _guestProfileQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<LoyaltyService> _logger;

        // Points required per tier threshold
        private const int SilverThreshold = 500;
        private const int GoldThreshold = 1500;

        public LoyaltyService(
            IDBCommandRepository<LoyaltyProgram> loyaltyCommandRepository,
            IDBQueryRepository<LoyaltyProgram> loyaltyQueryRepository,
            IDBCommandRepository<GuestProfile> guestProfileCommandRepository,
            IDBQueryRepository<GuestProfile> guestProfileQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            UserManager<ApplicationUser> userManager,
            ILogger<LoyaltyService> logger)
        {
            _loyaltyCommandRepository = loyaltyCommandRepository;
            _loyaltyQueryRepository = loyaltyQueryRepository;
            _guestProfileCommandRepository = guestProfileCommandRepository;
            _guestProfileQueryRepository = guestProfileQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<BaseResponse<LoyaltyResponseDTO>> GetLoyaltyByUserIdAsync(long userId)
        {
            var record = await _loyaltyQueryRepository.GetByDefaultAsync(l => l.UserId == userId && !l.IsDeleted);
            if (record == null)
                return BaseResponse<LoyaltyResponseDTO>.Failure(new LoyaltyResponseDTO(), ResponseMessages.LoyaltyRecordNotFound, ResponseStatusCode.NoRecordFound);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var response = BuildLoyaltyResponse(record, user);
            return BaseResponse<LoyaltyResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<LoyaltyResponseDTO>> AccruePointsAsync(long userId, int points, string reason)
        {
            _logger.LogInformation("Accruing {Points} points for user {UserId}", points, userId);

            var record = await _loyaltyQueryRepository.GetByDefaultAsync(l => l.UserId == userId && !l.IsDeleted);
            if (record == null)
            {
                // Create a new loyalty record for this user
                record = new LoyaltyProgram
                {
                    UserId = userId,
                    PointsEarned = points,
                    PointsRedeemed = 0,
                    CreatedBy = "System",
                    CreationDate = DateTime.UtcNow
                };
                _loyaltyCommandRepository.Add(record);
                await _loyaltyCommandRepository.SaveAsync();
            }
            else
            {
                record.PointsEarned += points;
                record.LastModifiedDate = DateTime.UtcNow;
                await _loyaltyCommandRepository.UpdateAsync(record);
            }

            // Sync loyalty tier on GuestProfile
            int balance = record.PointsEarned - record.PointsRedeemed;
            await SyncGuestTierAsync(userId, balance);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var response = BuildLoyaltyResponse(record, user);
            return BaseResponse<LoyaltyResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<LoyaltyResponseDTO>> RedeemPointsAsync(long userId, int points, long reservationId, AuditLog auditLog)
        {
            _logger.LogInformation("Redeeming {Points} points for user {UserId}, reservation {ReservationId}", points, userId, reservationId);

            var record = await _loyaltyQueryRepository.GetByDefaultAsync(l => l.UserId == userId && !l.IsDeleted);
            if (record == null)
                return BaseResponse<LoyaltyResponseDTO>.Failure(new LoyaltyResponseDTO(), ResponseMessages.LoyaltyRecordNotFound, ResponseStatusCode.NoRecordFound);

            int balance = record.PointsEarned - record.PointsRedeemed;
            if (points > balance)
                return BaseResponse<LoyaltyResponseDTO>.Failure(new LoyaltyResponseDTO(), ResponseMessages.InsufficientLoyaltyPoints, ResponseStatusCode.InvalidData);

            record.PointsRedeemed += points;
            record.LastModifiedDate = DateTime.UtcNow;
            await _loyaltyCommandRepository.UpdateAsync(record);

            // Sync tier after redemption
            int newBalance = record.PointsEarned - record.PointsRedeemed;
            await SyncGuestTierAsync(userId, newBalance);

            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            var user = await _userManager.FindByIdAsync(userId.ToString());
            var response = BuildLoyaltyResponse(record, user);
            return BaseResponse<LoyaltyResponseDTO>.Success(response, ResponseMessages.LoyaltyPointsRedeemed, ResponseStatusCode.OperationSuccessful);
        }

        private async Task SyncGuestTierAsync(long userId, int balance)
        {
            var profile = await _guestProfileQueryRepository.GetByDefaultAsync(g => g.UserId == userId && !g.IsDeleted);
            if (profile == null) return;

            profile.LoyaltyPoints = balance;
            profile.LoyaltyTier = balance >= GoldThreshold ? "Gold" : balance >= SilverThreshold ? "Silver" : "Bronze";
            profile.LastModifiedDate = DateTime.UtcNow;
            await _guestProfileCommandRepository.UpdateAsync(profile);
        }

        private static string ResolveTier(int balance) =>
            balance >= GoldThreshold ? "Gold" : balance >= SilverThreshold ? "Silver" : "Bronze";

        private LoyaltyResponseDTO BuildLoyaltyResponse(LoyaltyProgram record, ApplicationUser? user)
        {
            int balance = record.PointsEarned - record.PointsRedeemed;
            return new LoyaltyResponseDTO
            {
                Id = record.Id,
                UserId = record.UserId,
                GuestName = user?.FullName,
                GuestEmail = user?.Email,
                PointsEarned = record.PointsEarned,
                PointsRedeemed = record.PointsRedeemed,
                PointsBalance = balance,
                Tier = ResolveTier(balance)
            };
        }
    }
}
