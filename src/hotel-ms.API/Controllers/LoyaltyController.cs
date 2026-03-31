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
    [Route("api/v1/loyalty")]
    [ApiController]
    [Authorize]
    public class LoyaltyController : ControllerBase
    {
        private readonly ILoyaltyService _loyaltyService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public LoyaltyController(ILoyaltyService loyaltyService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _loyaltyService = loyaltyService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpGet("{userId}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<LoyaltyResponseDTO>))]
        public async Task<IActionResult> GetLoyalty(long userId)
        {
            var result = await _loyaltyService.GetLoyaltyByUserIdAsync(userId);
            return Ok(result);
        }

        [HttpPost("{userId}/accrue")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<LoyaltyResponseDTO>))]
        public async Task<IActionResult> AccruePoints(long userId, [FromBody] AccruePointsRequestDTO request)
        {
            var result = await _loyaltyService.AccruePointsAsync(userId, request.Points, request.Reason);
            return Ok(result);
        }

        [HttpPost("{userId}/redeem")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<LoyaltyResponseDTO>))]
        public async Task<IActionResult> RedeemPoints(long userId, [FromBody] RedeemPointsRequestDTO request)
        {
            var auditLog = BuildAuditLog(UserAction.RedeemLoyaltyPoints, userId.ToString());
            var result = await _loyaltyService.RedeemPointsAsync(userId, request.Points, request.ReservationId, auditLog);
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
