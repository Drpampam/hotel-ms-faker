using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Commands.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.Extensions.Logging;

namespace hotelier_core_app.Service.Implementation
{
    public class ExpenseService : IExpenseService
    {
        private readonly IDBCommandRepository<ReservationExpense> _expenseCommandRepository;
        private readonly IDBQueryRepository<ReservationExpense> _expenseQueryRepository;
        private readonly IDBQueryRepository<Reservation> _reservationQueryRepository;
        private readonly IDBCommandRepository<AuditLog> _auditLogCommandRepository;
        private readonly ILogger<ExpenseService> _logger;

        public ExpenseService(
            IDBCommandRepository<ReservationExpense> expenseCommandRepository,
            IDBQueryRepository<ReservationExpense> expenseQueryRepository,
            IDBQueryRepository<Reservation> reservationQueryRepository,
            IDBCommandRepository<AuditLog> auditLogCommandRepository,
            ILogger<ExpenseService> logger)
        {
            _expenseCommandRepository = expenseCommandRepository;
            _expenseQueryRepository = expenseQueryRepository;
            _reservationQueryRepository = reservationQueryRepository;
            _auditLogCommandRepository = auditLogCommandRepository;
            _logger = logger;
        }

        public async Task<BaseResponse<ReservationExpenseResponseDTO>> AddExpenseAsync(long reservationId, AddReservationExpenseDTO request, AuditLog auditLog)
        {
            var reservation = await _reservationQueryRepository.FindAsync(reservationId);
            if (reservation == null)
                return BaseResponse<ReservationExpenseResponseDTO>.Failure(new ReservationExpenseResponseDTO(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            var expense = new ReservationExpense
            {
                ReservationId = reservationId,
                Description = request.Description,
                Category = request.Category,
                Quantity = request.Quantity,
                UnitPrice = request.UnitPrice,
                Amount = request.Quantity * request.UnitPrice,
                CreatedBy = auditLog.PerformedBy,
                CreationDate = DateTime.UtcNow,
                IsDeleted = false
            };

            _expenseCommandRepository.Add(expense);
            _auditLogCommandRepository.Add(auditLog);
            await _expenseCommandRepository.SaveAsync();

            _logger.LogInformation("Expense added to reservation {ReservationId}", reservationId);

            return BaseResponse<ReservationExpenseResponseDTO>.Success(MapToDTO(expense), ResponseMessages.ExpenseAdded, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<List<ReservationExpenseResponseDTO>>> GetExpensesAsync(long reservationId)
        {
            var reservation = await _reservationQueryRepository.FindAsync(reservationId);
            if (reservation == null)
                return BaseResponse<List<ReservationExpenseResponseDTO>>.Failure(new List<ReservationExpenseResponseDTO>(), ResponseMessages.ReservationNotFound, ResponseStatusCode.ReservationNotFound);

            var expenses = await _expenseQueryRepository.GetByAsync(e => e.ReservationId == reservationId && !e.IsDeleted);
            var result = expenses.Select(MapToDTO).ToList();

            return BaseResponse<List<ReservationExpenseResponseDTO>>.Success(result, ResponseMessages.ExpensesRetrieved, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<bool>> DeleteExpenseAsync(long reservationId, long expenseId, AuditLog auditLog)
        {
            var expense = await _expenseQueryRepository.FindAsync(expenseId);
            if (expense == null || expense.IsDeleted)
                return BaseResponse<bool>.Failure(false, ResponseMessages.ExpenseNotFound, ResponseStatusCode.ExpenseNotFound);

            if (expense.ReservationId != reservationId)
                return BaseResponse<bool>.Failure(false, ResponseMessages.ExpenseReservationMismatch, ResponseStatusCode.ExpenseNotFound);

            expense.IsDeleted = true;
            await _expenseCommandRepository.UpdateAsync(expense);
            _auditLogCommandRepository.Add(auditLog);
            await _auditLogCommandRepository.SaveAsync();

            _logger.LogInformation("Expense {ExpenseId} deleted from reservation {ReservationId}", expenseId, reservationId);

            return BaseResponse<bool>.Success(true, ResponseMessages.ExpenseDeleted, ResponseStatusCode.OperationSuccessful);
        }

        private static ReservationExpenseResponseDTO MapToDTO(ReservationExpense e) => new ReservationExpenseResponseDTO
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
        };
    }
}
