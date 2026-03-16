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
    public class ServiceRequestServiceTests
    {
        private readonly IDBCommandRepository<ServiceRequest> _serviceRequestCommandRepo = Substitute.For<IDBCommandRepository<ServiceRequest>>();
        private readonly IDBQueryRepository<ServiceRequest> _serviceRequestQueryRepo = Substitute.For<IDBQueryRepository<ServiceRequest>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        private ServiceRequestService CreateService() => new(
            _serviceRequestCommandRepo,
            _serviceRequestQueryRepo,
            _auditLogCommandRepo,
            _mapper);

        [Fact]
        public async Task ChangeServiceRequestStateAsync_ShouldReturnFailure_WhenServiceRequestNotFound()
        {
            _serviceRequestQueryRepo.FindAsync(Arg.Any<long>()).Returns((ServiceRequest)null);
            var service = CreateService();
            var result = await service.ChangeServiceRequestStateAsync(1, ServiceRequestTrigger.Start);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task ChangeServiceRequestStateAsync_ShouldReturnFailure_WhenTriggerInvalid()
        {
            var sr = new ServiceRequest { Id = 1, ServiceRequestState = ServiceRequestState.Requested };
            _serviceRequestQueryRepo.FindAsync(Arg.Any<long>()).Returns(sr);
            sr.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.ChangeServiceRequestStateAsync(1, (ServiceRequestTrigger)999);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvalidData, result.StatusCode);
        }

        [Fact]
        public async Task ChangeServiceRequestStateAsync_ShouldReturnSuccess_WhenTriggerValid()
        {
            var sr = new ServiceRequest { Id = 1, ServiceRequestState = ServiceRequestState.Requested };
            _serviceRequestQueryRepo.FindAsync(Arg.Any<long>()).Returns(sr);
            sr.ConfigureStateMachine();
            _serviceRequestCommandRepo.UpdateAsync(sr).Returns(Task.CompletedTask);
            var service = CreateService();
            var result = await service.ChangeServiceRequestStateAsync(1, ServiceRequestTrigger.Start);
            Assert.True(result.Status);
            Assert.Equal(ServiceRequestState.InProgress, result.Data.State); // Assuming Start moves to InProgress
        }

        [Fact]
        public async Task GetServiceRequestStateAsync_ShouldReturnFailure_WhenServiceRequestNotFound()
        {
            _serviceRequestQueryRepo.FindAsync(Arg.Any<long>()).Returns((ServiceRequest)null);
            var service = CreateService();
            var result = await service.GetServiceRequestStateAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task GetServiceRequestStateAsync_ShouldReturnSuccess_WhenServiceRequestFound()
        {
            var sr = new ServiceRequest { Id = 1, ServiceRequestState = ServiceRequestState.Requested };
            _serviceRequestQueryRepo.FindAsync(Arg.Any<long>()).Returns(sr);
            sr.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.GetServiceRequestStateAsync(1);
            Assert.True(result.Status);
            Assert.Equal(ServiceRequestState.Requested, result.Data.State);
        }

        [Fact]
        public async Task GetAvailableTriggersAsync_ShouldReturnFailure_WhenServiceRequestNotFound()
        {
            _serviceRequestQueryRepo.FindAsync(Arg.Any<long>()).Returns((ServiceRequest)null);
            var service = CreateService();
            var result = await service.GetAvailableTriggersAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task GetAvailableTriggersAsync_ShouldReturnSuccess_WhenServiceRequestFound()
        {
            var sr = new ServiceRequest { Id = 1, ServiceRequestState = ServiceRequestState.Requested };
            _serviceRequestQueryRepo.FindAsync(Arg.Any<long>()).Returns(sr);
            sr.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.GetAvailableTriggersAsync(1);
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
        }
    }
}
