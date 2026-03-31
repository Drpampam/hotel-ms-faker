using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class LoyaltyServiceTests
    {
        private readonly IDBCommandRepository<LoyaltyProgram> _loyaltyCommandRepo = Substitute.For<IDBCommandRepository<LoyaltyProgram>>();
        private readonly IDBQueryRepository<LoyaltyProgram> _loyaltyQueryRepo = Substitute.For<IDBQueryRepository<LoyaltyProgram>>();
        private readonly IDBCommandRepository<GuestProfile> _guestProfileCommandRepo = Substitute.For<IDBCommandRepository<GuestProfile>>();
        private readonly IDBQueryRepository<GuestProfile> _guestProfileQueryRepo = Substitute.For<IDBQueryRepository<GuestProfile>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        private readonly ILogger<LoyaltyService> _logger = Substitute.For<ILogger<LoyaltyService>>();

        private LoyaltyService CreateService() => new(
            _loyaltyCommandRepo, _loyaltyQueryRepo,
            _guestProfileCommandRepo, _guestProfileQueryRepo,
            _auditLogCommandRepo, _userManager, _logger);

        [Fact]
        public async Task GetLoyaltyByUserIdAsync_ShouldReturnFailure_WhenNotFound()
        {
            _loyaltyQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<LoyaltyProgram, bool>>>())
                .Returns((LoyaltyProgram)null);
            var result = await CreateService().GetLoyaltyByUserIdAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task RedeemPointsAsync_ShouldReturnFailure_WhenInsufficientPoints()
        {
            var record = new LoyaltyProgram { UserId = 1, PointsEarned = 100, PointsRedeemed = 0 };
            _loyaltyQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<LoyaltyProgram, bool>>>()).Returns(record);
            var result = await CreateService().RedeemPointsAsync(1, 500, 1, new AuditLog());
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvalidData, result.StatusCode);
        }

        [Fact]
        public async Task AccruePointsAsync_ShouldCreateNewRecord_WhenNoneExists()
        {
            _loyaltyQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<LoyaltyProgram, bool>>>())
                .Returns((LoyaltyProgram)null);
            _guestProfileQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<GuestProfile, bool>>>())
                .Returns((GuestProfile)null);
            _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser)null);

            var result = await CreateService().AccruePointsAsync(1, 200, "Booking bonus");

            Assert.True(result.Status);
            _loyaltyCommandRepo.Received(1).Add(Arg.Any<LoyaltyProgram>());
        }

        [Fact]
        public async Task AccruePointsAsync_ShouldUpdateExistingRecord_WhenRecordExists()
        {
            var record = new LoyaltyProgram { UserId = 1, PointsEarned = 300, PointsRedeemed = 0 };
            _loyaltyQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<LoyaltyProgram, bool>>>()).Returns(record);
            _guestProfileQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<GuestProfile, bool>>>())
                .Returns((GuestProfile)null);
            _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser)null);

            var result = await CreateService().AccruePointsAsync(1, 200, "Stay bonus");

            Assert.True(result.Status);
            Assert.Equal(500, result.Data.PointsEarned);
            Assert.Equal("Silver", result.Data.Tier);
        }

        [Fact]
        public async Task RedeemPointsAsync_ShouldSucceed_WhenSufficientBalance()
        {
            var record = new LoyaltyProgram { UserId = 1, PointsEarned = 600, PointsRedeemed = 0 };
            _loyaltyQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<LoyaltyProgram, bool>>>()).Returns(record);
            _guestProfileQueryRepo.GetByDefaultAsync(Arg.Any<Expression<Func<GuestProfile, bool>>>())
                .Returns((GuestProfile)null);
            _userManager.FindByIdAsync(Arg.Any<string>()).Returns((ApplicationUser)null);

            var result = await CreateService().RedeemPointsAsync(1, 100, 1, new AuditLog());

            Assert.True(result.Status);
            Assert.Equal(500, result.Data.PointsBalance);
        }
    }
}
