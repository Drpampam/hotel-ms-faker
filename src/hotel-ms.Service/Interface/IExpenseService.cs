using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;

namespace hotelier_core_app.Service.Interface
{
    public interface IExpenseService : IAutoDependencyService
    {
        Task<BaseResponse<ReservationExpenseResponseDTO>> AddExpenseAsync(long reservationId, AddReservationExpenseDTO request, AuditLog auditLog);
        Task<BaseResponse<List<ReservationExpenseResponseDTO>>> GetExpensesAsync(long reservationId);
        Task<BaseResponse<bool>> DeleteExpenseAsync(long reservationId, long expenseId, AuditLog auditLog);
    }
}
