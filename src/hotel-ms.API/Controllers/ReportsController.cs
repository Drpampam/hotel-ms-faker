using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Service.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace hotelier_core_app.API.Controllers
{
    [Route("api/v1/reports")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("occupancy")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<OccupancyReportDTO>))]
        public async Task<IActionResult> GetOccupancyReport(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] long? propertyId = null)
        {
            var result = await _reportService.GetOccupancyReportAsync(fromDate, toDate, propertyId);
            return Ok(result);
        }

        [HttpGet("revenue")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<RevenueSummaryDTO>))]
        public async Task<IActionResult> GetRevenueSummary(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] long? propertyId = null)
        {
            var result = await _reportService.GetRevenueSummaryAsync(fromDate, toDate, propertyId);
            return Ok(result);
        }

        [HttpGet("reservations")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ReservationStatsDTO>))]
        public async Task<IActionResult> GetReservationStats(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] long? propertyId = null)
        {
            var result = await _reportService.GetReservationStatsAsync(fromDate, toDate, propertyId);
            return Ok(result);
        }

        [HttpGet("housekeeping")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<HousekeepingStatsDTO>))]
        public async Task<IActionResult> GetHousekeepingStats([FromQuery] DateTime date)
        {
            var result = await _reportService.GetHousekeepingStatsAsync(date);
            return Ok(result);
        }

        [HttpGet("payments")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<PaymentBreakdownDTO>))]
        public async Task<IActionResult> GetPaymentBreakdown(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate)
        {
            var result = await _reportService.GetPaymentBreakdownAsync(fromDate, toDate);
            return Ok(result);
        }

        [HttpGet("front-desk")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<FrontDeskSummaryDTO>))]
        public async Task<IActionResult> GetFrontDeskSummary([FromQuery] DateTime? date = null)
        {
            var result = await _reportService.GetFrontDeskSummaryAsync(date ?? DateTime.UtcNow);
            return Ok(result);
        }

        [HttpGet("expenses")]
        [Authorize(Roles = "Admin,SuperAdmin,Developer")]
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(BaseResponse<ExpenseReportDTO>))]
        public async Task<IActionResult> GetExpenseReport(
            [FromQuery] DateTime fromDate,
            [FromQuery] DateTime toDate,
            [FromQuery] string? search = null,
            [FromQuery] long? reservationId = null)
        {
            var result = await _reportService.GetExpenseReportAsync(fromDate, toDate, search, reservationId);
            return Ok(result);
        }
    }
}
