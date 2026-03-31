using hotelier_core_app.API.Helpers;
using hotelier_core_app.Core.Constants;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/billing")]
    [ApiController]
    [Authorize]
    public class BillingController : ControllerBase
    {
        private readonly IBillingService _billingService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public BillingController(IBillingService billingService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _billingService = billingService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost("invoices/generate/{reservationId}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<InvoiceResponseDTO>))]
        public async Task<IActionResult> GenerateInvoice(long reservationId)
        {
            var auditLog = BuildAuditLog(UserAction.GenerateInvoice, reservationId.ToString());
            var result = await _billingService.GenerateInvoiceAsync(reservationId, auditLog);
            return Ok(result);
        }

        [HttpGet("invoices/{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<InvoiceResponseDTO>))]
        public async Task<IActionResult> GetInvoice(long id)
        {
            var result = await _billingService.GetInvoiceByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("invoices/by-reservation/{reservationId}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<InvoiceResponseDTO>))]
        public async Task<IActionResult> GetInvoiceByReservation(long reservationId)
        {
            var result = await _billingService.GetInvoiceByReservationIdAsync(reservationId);
            return Ok(result);
        }

        [HttpGet("invoices")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<InvoiceResponseDTO>>))]
        public async Task<IActionResult> GetInvoices([FromQuery] GetInvoicesInputDTO input)
        {
            var result = await _billingService.GetInvoicesAsync(input);
            return Ok(result);
        }

        [HttpPost("invoices/{id}/mark-paid")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<InvoiceResponseDTO>))]
        public async Task<IActionResult> MarkPaid(long id)
        {
            var auditLog = BuildAuditLog(UserAction.GenerateInvoice, id.ToString());
            var result = await _billingService.MarkInvoicePaidAsync(id, auditLog);
            return Ok(result);
        }

        [HttpPost("invoices/{id}/void")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<InvoiceResponseDTO>))]
        public async Task<IActionResult> VoidInvoice(long id)
        {
            var auditLog = BuildAuditLog(UserAction.VoidInvoice, id.ToString());
            var result = await _billingService.VoidInvoiceAsync(id, auditLog);
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
