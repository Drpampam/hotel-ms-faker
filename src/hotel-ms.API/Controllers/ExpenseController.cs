using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/reservations/{reservationId}/expenses")]
    [ApiController]
    [Authorize]
    public class ExpenseController : ControllerBase
    {
        private readonly IExpenseService _expenseService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public ExpenseController(IExpenseService expenseService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _expenseService = expenseService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationExpenseResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> AddExpense(long reservationId, [FromBody] AddReservationExpenseDTO request)
        {
            var auditLog = BuildAuditLog(UserAction.AddReservationExpense, reservationId.ToString());
            var result = await _expenseService.AddExpenseAsync(reservationId, request, auditLog);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<List<ReservationExpenseResponseDTO>>))]
        public async Task<IActionResult> GetExpenses(long reservationId)
        {
            var result = await _expenseService.GetExpensesAsync(reservationId);
            return Ok(result);
        }

        [HttpDelete("{expenseId}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<bool>))]
        public async Task<IActionResult> DeleteExpense(long reservationId, long expenseId)
        {
            var auditLog = BuildAuditLog(UserAction.DeleteReservationExpense, expenseId.ToString());
            var result = await _expenseService.DeleteExpenseAsync(reservationId, expenseId, auditLog);
            return Ok(result);
        }

        private AuditLog BuildAuditLog(string action, string performedAgainst) => new AuditLog
        {
            Action = action,
            DatePerformed = DateTime.UtcNow,
            PerformedBy = _tokenHelper.GetUserFullName(Request),
            IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
            PerformerEmail = _tokenHelper.GetUserEmail(Request),
            PerformedAgainst = performedAgainst,
            MacAddress = _tokenHelper.GetMacAddress(Request)
        };
    }
}
