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
    public class PaymentServiceTests
    {
        private readonly IDBCommandRepository<Payment> _paymentCommandRepo = Substitute.For<IDBCommandRepository<Payment>>();
        private readonly IDBQueryRepository<Payment> _paymentQueryRepo = Substitute.For<IDBQueryRepository<Payment>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly IMapper _mapper = Substitute.For<IMapper>();

        private PaymentService CreateService() => new(
            _paymentCommandRepo,
            _paymentQueryRepo,
            _auditLogCommandRepo,
            _mapper);

        [Fact]
        public async Task GetPaymentStateAsync_ShouldReturnFailure_WhenPaymentNotFound()
        {
            _paymentQueryRepo.FindAsync(Arg.Any<long>()).Returns((Payment)null);
            var service = CreateService();
            var result = await service.GetPaymentStateAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task GetPaymentStateAsync_ShouldReturnSuccess_WhenPaymentFound()
        {
            var payment = new Payment { Id = 1, PaymentState = PaymentState.Pending };
            _paymentQueryRepo.FindAsync(Arg.Any<long>()).Returns(payment);
            payment.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.GetPaymentStateAsync(1);
            Assert.True(result.Status);
            Assert.Equal(PaymentState.Pending, result.Data.State);
        }

        [Fact]
        public async Task ChangePaymentStateAsync_ShouldReturnFailure_WhenPaymentNotFound()
        {
            _paymentQueryRepo.FindAsync(Arg.Any<long>()).Returns((Payment)null);
            var service = CreateService();
            var result = await service.ChangePaymentStateAsync(1, PaymentTrigger.Process);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task ChangePaymentStateAsync_ShouldReturnFailure_WhenTriggerInvalid()
        {
            var payment = new Payment { Id = 1, PaymentState = PaymentState.Pending };
            _paymentQueryRepo.FindAsync(Arg.Any<long>()).Returns(payment);
            payment.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.ChangePaymentStateAsync(1, (PaymentTrigger)999);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvalidData, result.StatusCode);
        }

        [Fact]
        public async Task ChangePaymentStateAsync_ShouldReturnSuccess_WhenTriggerValid()
        {
            var payment = new Payment { Id = 1, PaymentState = PaymentState.Pending };
            _paymentQueryRepo.FindAsync(Arg.Any<long>()).Returns(payment);
            payment.ConfigureStateMachine();
            _paymentCommandRepo.UpdateAsync(payment).Returns(Task.CompletedTask);
            var service = CreateService();
            var result = await service.ChangePaymentStateAsync(1, PaymentTrigger.Process);
            Assert.True(result.Status);
            Assert.Equal(PaymentState.Processing, result.Data.State);
        }

        [Fact]
        public async Task GetAvailableTriggersAsync_ShouldReturnFailure_WhenPaymentNotFound()
        {
            _paymentQueryRepo.FindAsync(Arg.Any<long>()).Returns((Payment)null);
            var service = CreateService();
            var result = await service.GetAvailableTriggersAsync(1);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.NoRecordFound, result.StatusCode);
        }

        [Fact]
        public async Task GetAvailableTriggersAsync_ShouldReturnSuccess_WhenPaymentFound()
        {
            var payment = new Payment { Id = 1, PaymentState = PaymentState.Pending };
            _paymentQueryRepo.FindAsync(Arg.Any<long>()).Returns(payment);
            payment.ConfigureStateMachine();
            var service = CreateService();
            var result = await service.GetAvailableTriggersAsync(1);
            Assert.True(result.Status);
            Assert.NotNull(result.Data);
        }
    }
}
