using hotelier_core_app.Model.DTOs.Response;

namespace hotelier_core_app.Service.Interface
{
    public interface IReportService : IAutoDependencyService
    {
        Task<BaseResponse<OccupancyReportDTO>> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate, long? propertyId = null);
        Task<BaseResponse<RevenueSummaryDTO>> GetRevenueSummaryAsync(DateTime fromDate, DateTime toDate, long? propertyId = null);
        Task<BaseResponse<ReservationStatsDTO>> GetReservationStatsAsync(DateTime fromDate, DateTime toDate, long? propertyId = null);
        Task<BaseResponse<HousekeepingStatsDTO>> GetHousekeepingStatsAsync(DateTime date);
        Task<BaseResponse<PaymentBreakdownDTO>> GetPaymentBreakdownAsync(DateTime fromDate, DateTime toDate);
        Task<BaseResponse<FrontDeskSummaryDTO>> GetFrontDeskSummaryAsync(DateTime date);
    }
}
