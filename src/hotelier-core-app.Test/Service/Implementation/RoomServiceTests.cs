using AutoMapper;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.States;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Implementation;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class RoomServiceTests
    {
        private readonly IDBCommandRepository<Room> _roomCommandRepo = Substitute.For<IDBCommandRepository<Room>>();
        private readonly IDBQueryRepository<Room> _roomQueryRepo = Substitute.For<IDBQueryRepository<Room>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        private RoomService CreateService() => new(
            _roomCommandRepo,
            _roomQueryRepo,
            _auditLogCommandRepo,
            _mapper);

        [Fact]
        public async Task GetRoomStateAsync_ShouldReturnFailure_WhenRoomNotFound()
        {
            _roomQueryRepo.FindAsync(Arg.Any<long>()).Returns((Room)null);
            var service = CreateService();
            var result = await service.GetRoomStateAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task GetRoomStateAsync_ShouldReturnSuccess_WhenRoomFound()
        {
            var room = new Room { Id = 1, RoomState = RoomState.Available };
            _roomQueryRepo.FindAsync(Arg.Any<long>()).Returns(room);
            room.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.GetRoomStateAsync(1);
            Assert.True(result.Status);
            Assert.Equal(RoomState.Available, result.Data.State);
        }

        [Fact]
        public async Task ChangeRoomStateAsync_ShouldReturnFailure_WhenRoomNotFound()
        {
            _roomQueryRepo.FindAsync(Arg.Any<long>()).Returns((Room)null);
            var service = CreateService();
            var result = await service.ChangeRoomStateAsync(1, RoomTrigger.CheckIn);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task ChangeRoomStateAsync_ShouldReturnFailure_WhenTriggerInvalid()
        {
            var room = new Room { Id = 1, RoomState = RoomState.Available };
            _roomQueryRepo.FindAsync(Arg.Any<long>()).Returns(room);
            room.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.ChangeRoomStateAsync(1, (RoomTrigger)999);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvalidData, result.StatusCode);
        }

        [Fact]
        public async Task ChangeRoomStateAsync_ShouldReturnSuccess_WhenTriggerValid()
        {
            var room = new Room { Id = 1, RoomState = RoomState.Available };
            _roomQueryRepo.FindAsync(Arg.Any<long>()).Returns(room);
            room.ConfigureStateMachine();
            _roomCommandRepo.UpdateAsync(room).Returns(Task.CompletedTask);
            var service = CreateService();
            var result = await service.ChangeRoomStateAsync(1, RoomTrigger.CheckIn);
            Assert.True(result.Status);
            Assert.Equal(RoomState.Occupied, result.Data.State); // Assuming CheckIn moves to Occupied
        }

        [Fact]
        public async Task GetAvailableTriggersAsync_ShouldReturnFailure_WhenRoomNotFound()
        {
            _roomQueryRepo.FindAsync(Arg.Any<long>()).Returns((Room)null);
            var service = CreateService();
            var result = await service.GetAvailableTriggersAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task GetAvailableTriggersAsync_ShouldReturnSuccess_WhenRoomFound()
        {
            var room = new Room { Id = 1, RoomState = RoomState.Available };
            _roomQueryRepo.FindAsync(Arg.Any<long>()).Returns(room);
            room.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.GetAvailableTriggersAsync(1);
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
        }
    }
}
