using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Core.States;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Implementation;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Threading.Tasks;
using Xunit;

namespace hotelier_core_app.Test.Service.Implementation
{
    public class BillingServiceTests
    {
        private readonly IDBCommandRepository<Invoice> _invoiceCommandRepo = Substitute.For<IDBCommandRepository<Invoice>>();
        private readonly IDBQueryRepository<Invoice> _invoiceQueryRepo = Substitute.For<IDBQueryRepository<Invoice>>();
        private readonly IDBCommandRepository<InvoiceLineItem> _lineItemCommandRepo = Substitute.For<IDBCommandRepository<InvoiceLineItem>>();
        private readonly IDBQueryRepository<Reservation> _reservationQueryRepo = Substitute.For<IDBQueryRepository<Reservation>>();
        private readonly IDBQueryRepository<Room> _roomQueryRepo = Substitute.For<IDBQueryRepository<Room>>();
        private readonly IDBQueryRepository<Payment> _paymentQueryRepo = Substitute.For<IDBQueryRepository<Payment>>();
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepo = Substitute.For<IDBCommandRepository<AuditLog>>();
        private readonly UserManager<ApplicationUser> _userManager = Substitute.For<UserManager<ApplicationUser>>(
            Substitute.For<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        private readonly IUtility _utility = Substitute.For<IUtility>();
        private readonly ILogger<BillingService> _logger = Substitute.For<ILogger<BillingService>>();

        private BillingService CreateService() => new(
            _invoiceCommandRepo, _invoiceQueryRepo, _lineItemCommandRepo,
            _reservationQueryRepo, _roomQueryRepo, _paymentQueryRepo,
            _auditLogCommandRepo, _userManager, _utility, _logger);

        [Fact]
        public async Task GenerateInvoiceAsync_ShouldReturnFailure_WhenReservationNotFound()
        {
            _reservationQueryRepo.FindAsync(Arg.Any<long>()).Returns((Reservation)null);
            var result = await CreateService().GenerateInvoiceAsync(1, new AuditLog());
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.ReservationNotFound, result.StatusCode);
        }

        [Fact]
        public async Task GetInvoiceByIdAsync_ShouldReturnFailure_WhenNotFound()
        {
            _invoiceQueryRepo.FindAsync(Arg.Any<long>()).Returns((Invoice)null);
            var result = await CreateService().GetInvoiceByIdAsync(99);
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvoiceNotFound, result.StatusCode);
        }

        [Fact]
        public async Task VoidInvoiceAsync_ShouldReturnFailure_WhenInvoiceNotFound()
        {
            _invoiceQueryRepo.FindAsync(Arg.Any<long>()).Returns((Invoice)null);
            var result = await CreateService().VoidInvoiceAsync(1, new AuditLog());
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvoiceNotFound, result.StatusCode);
        }

        [Fact]
        public async Task MarkInvoicePaidAsync_ShouldReturnFailure_WhenAlreadyPaid()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Paid };
            _invoiceQueryRepo.FindAsync(Arg.Any<long>()).Returns(invoice);
            var result = await CreateService().MarkInvoicePaidAsync(1, new AuditLog());
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvoiceAlreadyPaid, result.StatusCode);
        }

        [Fact]
        public async Task VoidInvoiceAsync_ShouldReturnFailure_WhenAlreadyVoided()
        {
            var invoice = new Invoice { Id = 1, Status = InvoiceStatus.Void };
            _invoiceQueryRepo.FindAsync(Arg.Any<long>()).Returns(invoice);
            var result = await CreateService().VoidInvoiceAsync(1, new AuditLog());
            Assert.False(result.Status);
            Assert.Equal(ResponseStatusCode.InvoiceAlreadyVoided, result.StatusCode);
        }
    }
}
