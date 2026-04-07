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
using Microsoft.Extensions.Logging;

namespace hotelier_core_app.Service.Implementation
{
    public class BillingService : IBillingService
    {
        private readonly IDBCommandRepository<Invoice> _invoiceCommandRepository;
        private readonly IDBQueryRepository<Invoice> _invoiceQueryRepository;
        private readonly IDBCommandRepository<InvoiceLineItem> _lineItemCommandRepository;
        private readonly IDBQueryRepository<Reservation> _reservationQueryRepository;
        private readonly IDBQueryRepository<Room> _roomQueryRepository;
        private readonly IDBQueryRepository<Payment> _paymentQueryRepository;
        private readonly IDBQueryRepository<ReservationExpense> _expenseQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUtility _utility;
        private readonly ILogger<BillingService> _logger;

        public BillingService(
            IDBCommandRepository<Invoice> invoiceCommandRepository,
            IDBQueryRepository<Invoice> invoiceQueryRepository,
            IDBCommandRepository<InvoiceLineItem> lineItemCommandRepository,
            IDBQueryRepository<Reservation> reservationQueryRepository,
            IDBQueryRepository<Room> roomQueryRepository,
            IDBQueryRepository<Payment> paymentQueryRepository,
            IDBQueryRepository<ReservationExpense> expenseQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            UserManager<ApplicationUser> userManager,
            IUtility utility,
            ILogger<BillingService> logger)
        {
            _invoiceCommandRepository = invoiceCommandRepository;
            _invoiceQueryRepository = invoiceQueryRepository;
            _lineItemCommandRepository = lineItemCommandRepository;
            _reservationQueryRepository = reservationQueryRepository;
            _roomQueryRepository = roomQueryRepository;
            _paymentQueryRepository = paymentQueryRepository;
            _expenseQueryRepository = expenseQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _userManager = userManager;
            _utility = utility;
            _logger = logger;
        }

        public async Task<BaseResponse<InvoiceResponseDTO>> GenerateInvoiceAsync(long reservationId, AuditLog auditLog)
        {
            _logger.LogInformation("Generating invoice for reservation {ReservationId}", reservationId);

            var reservation = await _reservationQueryRepository.FindAsync(reservationId);
            if (reservation == null)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            // Prevent duplicate invoices for the same reservation
            var existing = await _invoiceQueryRepository.GetByDefaultAsync(i => i.ReservationId == reservationId && !i.IsDeleted && i.Status != InvoiceStatus.Void);
            if (existing != null)
            {
                _logger.LogWarning("Invoice already exists for reservation {ReservationId}", reservationId);
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), "An active invoice already exists for this reservation", ResponseStatusCode.DuplicateKeyMessage);
            }

            var room = await _roomQueryRepository.FindAsync(reservation.RoomId);
            var guest = await _userManager.FindByIdAsync(reservation.GuestId.ToString());
            int nights = (int)(reservation.CheckOutDate.Date - reservation.CheckInDate.Date).TotalDays;

            var lineItems = new List<InvoiceLineItem>();

            // Room charges
            decimal roomTotal = nights * (room?.PricePerNight ?? 0);
            lineItems.Add(new InvoiceLineItem
            {
                Description = $"Room {room?.Number} ({room?.Type}) — {nights} night(s) @ {room?.PricePerNight:C}",
                Category = "Room",
                Quantity = nights,
                UnitPrice = room?.PricePerNight ?? 0,
                Amount = roomTotal
            });

            // Service charges (completed service requests)
            var serviceRequests = await GetServiceCharges(reservationId, lineItems);

            // Reservation expenses (food, laundry, minibar, etc.)
            var expenses = await _expenseQueryRepository.GetByAsync(e => e.ReservationId == reservationId && !e.IsDeleted);
            foreach (var expense in expenses)
            {
                lineItems.Add(new InvoiceLineItem
                {
                    Description = expense.Description,
                    Category = expense.Category ?? "Expense",
                    Quantity = expense.Quantity,
                    UnitPrice = expense.UnitPrice,
                    Amount = expense.Amount
                });
            }

            decimal subTotal = lineItems.Sum(l => l.Amount);
            decimal taxRate = 0.10m;  // 10% tax — configurable in future
            decimal taxAmount = Math.Round(subTotal * taxRate, 2);
            decimal discountAmount = reservation.TotalPrice < subTotal ? subTotal - reservation.TotalPrice : 0;
            decimal total = subTotal + taxAmount - discountAmount;

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{reservationId}",
                ReservationId = reservationId,
                GuestId = reservation.GuestId,
                IssueDate = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(7),
                SubTotal = subTotal,
                TaxAmount = taxAmount,
                DiscountAmount = discountAmount,
                TotalAmount = total,
                Status = InvoiceStatus.Issued,
                CreatedBy = auditLog.PerformedBy,
                CreationDate = DateTime.UtcNow
            };

            _invoiceCommandRepository.Add(invoice);
            await _invoiceCommandRepository.SaveAsync();

            foreach (var item in lineItems)
            {
                item.InvoiceId = invoice.Id;
                _lineItemCommandRepository.Add(item);
            }

            _auditLogCommandRepository.Add(auditLog);
            await _invoiceCommandRepository.SaveAsync();

            _logger.LogInformation("Invoice {InvoiceNumber} generated successfully", invoice.InvoiceNumber);

            var response = BuildInvoiceResponse(invoice, lineItems, guest);
            return BaseResponse<InvoiceResponseDTO>.Success(response, ResponseMessages.InvoiceGenerated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<InvoiceResponseDTO>> GetInvoiceByIdAsync(long invoiceId)
        {
            var invoice = _invoiceQueryRepository.FindByInclude(i => i.Id == invoiceId, i => i.LineItems).FirstOrDefault();
            if (invoice == null)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.InvoiceNotFound, ResponseStatusCode.InvoiceNotFound);

            var guest = await _userManager.FindByIdAsync(invoice.GuestId.ToString());
            var response = BuildInvoiceResponse(invoice, invoice.LineItems.ToList(), guest);
            return BaseResponse<InvoiceResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<InvoiceResponseDTO>> GetInvoiceByReservationIdAsync(long reservationId)
        {
            var invoice = _invoiceQueryRepository.FindByInclude(i => i.ReservationId == reservationId && !i.IsDeleted, i => i.LineItems).FirstOrDefault();
            if (invoice == null)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.InvoiceNotFound, ResponseStatusCode.InvoiceNotFound);

            var guest = await _userManager.FindByIdAsync(invoice.GuestId.ToString());
            var response = BuildInvoiceResponse(invoice, invoice.LineItems.ToList(), guest);
            return BaseResponse<InvoiceResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<PageBaseResponse<List<InvoiceResponseDTO>>> GetInvoicesAsync(GetInvoicesInputDTO input)
        {
            var all = await _invoiceQueryRepository.GetByAsync(i =>
                !i.IsDeleted &&
                (!input.GuestId.HasValue || i.GuestId == input.GuestId.Value) &&
                (!input.ReservationId.HasValue || i.ReservationId == input.ReservationId.Value) &&
                (!input.TenantId.HasValue || i.TenantId == input.TenantId.Value) &&
                (!input.Status.HasValue || i.Status == input.Status.Value) &&
                (!input.FromDate.HasValue || i.IssueDate >= input.FromDate.Value) &&
                (!input.ToDate.HasValue || i.IssueDate <= input.ToDate.Value));

            var paginated = _utility.Paginate(all, input.PageNumber, input.PageSize);
            var responses = new List<InvoiceResponseDTO>();
            foreach (var invoice in paginated)
            {
                var guest = await _userManager.FindByIdAsync(invoice.GuestId.ToString());
                responses.Add(BuildInvoiceResponse(invoice, invoice.LineItems?.ToList() ?? new(), guest));
            }

            return PageBaseResponse<List<InvoiceResponseDTO>>.Success(responses, ResponseMessages.InvoicesRetrieved,
                count: responses.Count, totalPageCount: all.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }

        public async Task<BaseResponse<InvoiceResponseDTO>> MarkInvoicePaidAsync(long invoiceId, AuditLog auditLog)
        {
            var invoice = await _invoiceQueryRepository.FindAsync(invoiceId);
            if (invoice == null)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.InvoiceNotFound, ResponseStatusCode.InvoiceNotFound);

            if (invoice.Status == InvoiceStatus.Paid)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.InvoiceAlreadyPaid, ResponseStatusCode.InvoiceAlreadyPaid);

            if (invoice.Status == InvoiceStatus.Void)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.InvoiceAlreadyVoided, ResponseStatusCode.InvoiceAlreadyVoided);

            invoice.Status = InvoiceStatus.Paid;
            invoice.ModifiedBy = auditLog.PerformedBy;
            invoice.LastModifiedDate = DateTime.UtcNow;

            await _invoiceCommandRepository.UpdateAsync(invoice);
            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            var guest = await _userManager.FindByIdAsync(invoice.GuestId.ToString());
            var response = BuildInvoiceResponse(invoice, invoice.LineItems?.ToList() ?? new(), guest);
            return BaseResponse<InvoiceResponseDTO>.Success(response, ResponseMessages.OperationSuccessful, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<InvoiceResponseDTO>> VoidInvoiceAsync(long invoiceId, AuditLog auditLog)
        {
            var invoice = await _invoiceQueryRepository.FindAsync(invoiceId);
            if (invoice == null)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.InvoiceNotFound, ResponseStatusCode.InvoiceNotFound);

            if (invoice.Status == InvoiceStatus.Void)
                return BaseResponse<InvoiceResponseDTO>.Failure(new InvoiceResponseDTO(), ResponseMessages.InvoiceAlreadyVoided, ResponseStatusCode.InvoiceAlreadyVoided);

            invoice.Status = InvoiceStatus.Void;
            invoice.ModifiedBy = auditLog.PerformedBy;
            invoice.LastModifiedDate = DateTime.UtcNow;

            await _invoiceCommandRepository.UpdateAsync(invoice);
            _auditLogCommandRepository.Add(auditLog);

            var guest = await _userManager.FindByIdAsync(invoice.GuestId.ToString());
            var response = BuildInvoiceResponse(invoice, invoice.LineItems?.ToList() ?? new(), guest);
            return BaseResponse<InvoiceResponseDTO>.Success(response, ResponseMessages.InvoiceVoided, ResponseStatusCode.OperationSuccessful);
        }

        private async Task<List<InvoiceLineItem>> GetServiceCharges(long reservationId, List<InvoiceLineItem> lineItems)
        {
            // Placeholder: in a full implementation, query service request charges here
            return lineItems;
        }

        private InvoiceResponseDTO BuildInvoiceResponse(Invoice invoice, List<InvoiceLineItem> lineItems, ApplicationUser? guest) => new InvoiceResponseDTO
        {
            Id = invoice.Id,
            InvoiceNumber = invoice.InvoiceNumber,
            ReservationId = invoice.ReservationId,
            GuestId = invoice.GuestId,
            GuestName = guest?.FullName,
            GuestEmail = guest?.Email,
            IssueDate = invoice.IssueDate,
            DueDate = invoice.DueDate,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            DiscountAmount = invoice.DiscountAmount,
            TotalAmount = invoice.TotalAmount,
            Status = invoice.Status,
            LineItems = lineItems.Select(l => new InvoiceLineItemDTO
            {
                Id = l.Id,
                Description = l.Description,
                Category = l.Category,
                Quantity = l.Quantity,
                UnitPrice = l.UnitPrice,
                Amount = l.Amount
            }).ToList(),
            CreationDate = invoice.CreationDate
        };
    }
}
