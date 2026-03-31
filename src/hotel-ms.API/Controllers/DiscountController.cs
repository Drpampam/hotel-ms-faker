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
    [Route("api/v1/discounts")]
    [ApiController]
    [Authorize]
    public class DiscountController : ControllerBase
    {
        private readonly IDiscountService _discountService;
        private readonly ITokenService _tokenHelper;
        private readonly IHttpContextAccessor _accessor;

        public DiscountController(IDiscountService discountService, ITokenService tokenHelper, IHttpContextAccessor accessor)
        {
            _discountService = discountService;
            _tokenHelper = tokenHelper;
            _accessor = accessor;
        }

        [HttpPost]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<DiscountResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> CreateDiscount(CreateDiscountRequestDTO request)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.CreateDiscount,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = request.Name,
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            var result = await _discountService.CreateDiscountAsync(request, auditLog);
            return Ok(result);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<DiscountResponseDTO>))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(ValidationResultModel))]
        public async Task<IActionResult> UpdateDiscount(UpdateDiscountRequestDTO request)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.UpdateDiscount,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = request.Id.ToString(),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            var result = await _discountService.UpdateDiscountAsync(request, auditLog);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<DiscountResponseDTO>))]
        public async Task<IActionResult> GetDiscount(long id)
        {
            var result = await _discountService.GetDiscountByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,SuperAdmin,FrontDesk,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(PageBaseResponse<List<DiscountResponseDTO>>))]
        public async Task<IActionResult> GetDiscounts([FromQuery] GetDiscountsInputDTO input)
        {
            var result = await _discountService.GetDiscountsAsync(input);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse))]
        public async Task<IActionResult> DeleteDiscount(long id)
        {
            var auditLog = new AuditLog
            {
                Action = UserAction.DeleteDiscount,
                DatePerformed = DateTime.UtcNow,
                PerformedBy = _tokenHelper.GetUserFullName(Request),
                IpAddress = _accessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown IP",
                PerformerEmail = _tokenHelper.GetUserEmail(Request),
                PerformedAgainst = id.ToString(),
                MacAddress = _tokenHelper.GetMacAddress(Request)
            };
            var result = await _discountService.DeleteDiscountAsync(id, auditLog);
            return Ok(result);
        }
    }
}
