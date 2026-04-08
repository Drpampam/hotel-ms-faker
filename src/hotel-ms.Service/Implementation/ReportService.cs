using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Executers;
using hotelier_core_app.Migrations;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Service.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace hotelier_core_app.Service.Implementation
{
    public class ReportService : IReportService
    {
        private readonly IExecuters _executers;
        private readonly ITenantProvider _tenantProvider;
        private readonly string _connStr;
        private readonly ILogger<ReportService> _logger;

        public ReportService(IExecuters executers, ITenantProvider tenantProvider, IConfiguration configuration, ILogger<ReportService> logger)
        {
            _executers = executers;
            _tenantProvider = tenantProvider;
            _connStr = configuration.GetConnectionString("DbConnectionString")!;
            _logger = logger;
        }

        private string Schema => _tenantProvider.GetSchema();

        // Internal typed result classes to avoid dynamic casting issues with PostgreSQL Dapper
        private class OccupancyQueryResult
        {
            public long TotalRooms { get; set; }
            public long OccupiedRooms { get; set; }
            public long AvailableRooms { get; set; }
            public long CleaningRooms { get; set; }
            public long MaintenanceRooms { get; set; }
        }

        private class RevenueSummaryQueryResult
        {
            public decimal TotalRevenue { get; set; }
            public decimal RoomRevenue { get; set; }
            public decimal TaxCollected { get; set; }
            public decimal TotalDiscountsApplied { get; set; }
            public long PaidInvoicesCount { get; set; }
            public long PendingInvoicesCount { get; set; }
        }

        private class ReservationStatsQueryResult
        {
            public long TotalReservations { get; set; }
            public long PendingReservations { get; set; }
            public long ConfirmedReservations { get; set; }
            public long CheckedInCount { get; set; }
            public long CheckedOutCount { get; set; }
            public long CancelledCount { get; set; }
            public long NoShowCount { get; set; }
            public double AverageStayDays { get; set; }
        }

        private class HousekeepingStatsQueryResult
        {
            public long TotalTasks { get; set; }
            public long PendingTasks { get; set; }
            public long InProgressTasks { get; set; }
            public long CompletedTasks { get; set; }
            public long SkippedTasks { get; set; }
        }

        private class PaymentTotalsQueryResult
        {
            public long TotalPayments { get; set; }
            public decimal TotalAmount { get; set; }
        }

        private class ExpenseReportItemQueryResult
        {
            public long Id { get; set; }
            public long ReservationId { get; set; }
            public string? GuestName { get; set; }
            public string? GuestEmail { get; set; }
            public string? RoomNumber { get; set; }
            public string Description { get; set; } = string.Empty;
            public string? Category { get; set; }
            public int Quantity { get; set; }
            public decimal UnitPrice { get; set; }
            public decimal Amount { get; set; }
            public string? CreatedBy { get; set; }
            public DateTime CreationDate { get; set; }
        }

        private class ExpenseCategorySummaryQueryResult
        {
            public string Category { get; set; } = string.Empty;
            public long Count { get; set; }
            public decimal Amount { get; set; }
        }

        private class FrontDeskQueryResult
        {
            public long ExpectedArrivals { get; set; }
            public long ActualCheckIns { get; set; }
            public long ExpectedDepartures { get; set; }
            public long ActualCheckOuts { get; set; }
            public long CurrentlyOccupied { get; set; }
        }

        public async Task<BaseResponse<OccupancyReportDTO>> GetOccupancyReportAsync(DateTime fromDate, DateTime toDate, long? propertyId = null)
        {
            _logger.LogInformation("Generating {ReportType} report for schema {Schema}", "Occupancy", Schema);
            fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
            toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);
            var schema = Schema;
            var propertyFilter = propertyId.HasValue ? "AND r.\"PropertyId\" = @PropertyId" : "";

            var sql = $@"
                SELECT
                    COUNT(*) AS ""TotalRooms"",
                    COUNT(*) FILTER (WHERE r.""RoomState"" = 1) AS ""OccupiedRooms"",
                    COUNT(*) FILTER (WHERE r.""RoomState"" = 0) AS ""AvailableRooms"",
                    COUNT(*) FILTER (WHERE r.""RoomState"" = 2) AS ""CleaningRooms"",
                    COUNT(*) FILTER (WHERE r.""RoomState"" = 3) AS ""MaintenanceRooms""
                FROM ""{schema}"".""Room"" r
                WHERE r.""IsDeleted"" = false
                {propertyFilter}";

            var raw = await _executers.ExecuteSingleReaderAsync<OccupancyQueryResult>(_connStr, sql, new { PropertyId = propertyId });

            long total = raw?.TotalRooms ?? 0;
            long occupied = raw?.OccupiedRooms ?? 0;
            long available = raw?.AvailableRooms ?? 0;
            long cleaning = raw?.CleaningRooms ?? 0;
            long maintenance = raw?.MaintenanceRooms ?? 0;

            var dto = new OccupancyReportDTO
            {
                TotalRooms = (int)total,
                OccupiedRooms = (int)occupied,
                AvailableRooms = (int)available,
                CleaningRooms = (int)cleaning,
                MaintenanceRooms = (int)maintenance,
                OccupancyRate = total > 0 ? Math.Round((double)occupied / total * 100, 2) : 0,
                FromDate = fromDate,
                ToDate = toDate
            };

            return BaseResponse<OccupancyReportDTO>.Success(dto, ResponseMessages.ReportGenerated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<RevenueSummaryDTO>> GetRevenueSummaryAsync(DateTime fromDate, DateTime toDate, long? propertyId = null)
        {
            _logger.LogInformation("Generating {ReportType} report for schema {Schema}", "Revenue", Schema);
            fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
            toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);
            var schema = Schema;
            var propertyJoin = propertyId.HasValue
                ? $@"INNER JOIN ""{schema}"".""Reservation"" res ON i.""ReservationId"" = res.""Id"" AND res.""IsDeleted"" = false
                     INNER JOIN ""{schema}"".""Room"" rm ON res.""RoomId"" = rm.""Id"" AND rm.""PropertyId"" = @PropertyId"
                : "";

            var sql = $@"
                SELECT
                    COALESCE(SUM(i.""TotalAmount""), 0) AS ""TotalRevenue"",
                    COALESCE(SUM(i.""SubTotal""), 0) AS ""RoomRevenue"",
                    COALESCE(SUM(i.""TaxAmount""), 0) AS ""TaxCollected"",
                    COALESCE(SUM(i.""DiscountAmount""), 0) AS ""TotalDiscountsApplied"",
                    COUNT(*) FILTER (WHERE i.""Status"" = 2) AS ""PaidInvoicesCount"",
                    COUNT(*) FILTER (WHERE i.""Status"" IN (0, 1)) AS ""PendingInvoicesCount""
                FROM ""{schema}"".""Invoice"" i
                {propertyJoin}
                WHERE i.""IsDeleted"" = false
                  AND i.""IssueDate"" >= @FromDate
                  AND i.""IssueDate"" <= @ToDate";

            var raw = await _executers.ExecuteSingleReaderAsync<RevenueSummaryQueryResult>(_connStr, sql, new { FromDate = fromDate, ToDate = toDate, PropertyId = propertyId });

            var dto = new RevenueSummaryDTO
            {
                TotalRevenue = raw?.TotalRevenue ?? 0,
                RoomRevenue = raw?.RoomRevenue ?? 0,
                TaxCollected = raw?.TaxCollected ?? 0,
                TotalDiscountsApplied = raw?.TotalDiscountsApplied ?? 0,
                PaidInvoicesCount = (int)(raw?.PaidInvoicesCount ?? 0),
                PendingInvoicesCount = (int)(raw?.PendingInvoicesCount ?? 0),
                FromDate = fromDate,
                ToDate = toDate
            };

            return BaseResponse<RevenueSummaryDTO>.Success(dto, ResponseMessages.ReportGenerated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<ReservationStatsDTO>> GetReservationStatsAsync(DateTime fromDate, DateTime toDate, long? propertyId = null)
        {
            _logger.LogInformation("Generating {ReportType} report for schema {Schema}", "ReservationStats", Schema);
            fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
            toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);
            var schema = Schema;
            var propertyJoin = propertyId.HasValue
                ? $@"INNER JOIN ""{schema}"".""Room"" rm ON r.""RoomId"" = rm.""Id"" AND rm.""PropertyId"" = @PropertyId"
                : "";

            var sql = $@"
                SELECT
                    COUNT(*) AS ""TotalReservations"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 0) AS ""PendingReservations"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 1) AS ""ConfirmedReservations"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 2) AS ""CheckedInCount"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 3) AS ""CheckedOutCount"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 4) AS ""CancelledCount"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 5) AS ""NoShowCount"",
                    COALESCE(AVG(EXTRACT(DAY FROM r.""CheckOutDate"" - r.""CheckInDate"")), 0) AS ""AverageStayDays""
                FROM ""{schema}"".""Reservation"" r
                {propertyJoin}
                WHERE r.""IsDeleted"" = false
                  AND r.""CheckInDate"" >= @FromDate
                  AND r.""CheckInDate"" <= @ToDate";

            var raw = await _executers.ExecuteSingleReaderAsync<ReservationStatsQueryResult>(_connStr, sql, new { FromDate = fromDate, ToDate = toDate, PropertyId = propertyId });

            var dto = new ReservationStatsDTO
            {
                TotalReservations = (int)(raw?.TotalReservations ?? 0),
                PendingReservations = (int)(raw?.PendingReservations ?? 0),
                ConfirmedReservations = (int)(raw?.ConfirmedReservations ?? 0),
                CheckedInCount = (int)(raw?.CheckedInCount ?? 0),
                CheckedOutCount = (int)(raw?.CheckedOutCount ?? 0),
                CancelledCount = (int)(raw?.CancelledCount ?? 0),
                NoShowCount = (int)(raw?.NoShowCount ?? 0),
                AverageStayDays = Math.Round(raw?.AverageStayDays ?? 0, 2),
                FromDate = fromDate,
                ToDate = toDate
            };

            return BaseResponse<ReservationStatsDTO>.Success(dto, ResponseMessages.ReportGenerated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<HousekeepingStatsDTO>> GetHousekeepingStatsAsync(DateTime date)
        {
            _logger.LogInformation("Generating {ReportType} report for schema {Schema}", "HousekeepingStats", Schema);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var schema = Schema;
            var sql = $@"
                SELECT
                    COUNT(*) AS ""TotalTasks"",
                    COUNT(*) FILTER (WHERE h.""State"" = 0) AS ""PendingTasks"",
                    COUNT(*) FILTER (WHERE h.""State"" = 1) AS ""InProgressTasks"",
                    COUNT(*) FILTER (WHERE h.""State"" = 2) AS ""CompletedTasks"",
                    COUNT(*) FILTER (WHERE h.""State"" = 3) AS ""SkippedTasks""
                FROM ""{schema}"".""HousekeepingTask"" h
                WHERE h.""IsDeleted"" = false
                  AND DATE(h.""ScheduledAt"") = DATE(@Date)";

            var raw = await _executers.ExecuteSingleReaderAsync<HousekeepingStatsQueryResult>(_connStr, sql, new { Date = date });

            long total = raw?.TotalTasks ?? 0;
            long completed = raw?.CompletedTasks ?? 0;

            var dto = new HousekeepingStatsDTO
            {
                TotalTasks = (int)total,
                PendingTasks = (int)(raw?.PendingTasks ?? 0),
                InProgressTasks = (int)(raw?.InProgressTasks ?? 0),
                CompletedTasks = (int)completed,
                SkippedTasks = (int)(raw?.SkippedTasks ?? 0),
                CompletionRate = total > 0 ? Math.Round((double)completed / total * 100, 2) : 0,
                Date = date.Date
            };

            return BaseResponse<HousekeepingStatsDTO>.Success(dto, ResponseMessages.ReportGenerated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<PaymentBreakdownDTO>> GetPaymentBreakdownAsync(DateTime fromDate, DateTime toDate)
        {
            _logger.LogInformation("Generating {ReportType} report for schema {Schema}", "PaymentBreakdown", Schema);
            fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
            toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);
            var schema = Schema;

            var totalSql = $@"
                SELECT
                    COUNT(*) AS ""TotalPayments"",
                    COALESCE(SUM(p.""Amount""), 0) AS ""TotalAmount""
                FROM ""{schema}"".""Payment"" p
                WHERE p.""PaymentDate"" >= @FromDate
                  AND p.""PaymentDate"" <= @ToDate";

            var byMethodSql = $@"
                SELECT
                    p.""PaymentMethod"" AS ""Method"",
                    COUNT(*) AS ""Count"",
                    COALESCE(SUM(p.""Amount""), 0) AS ""Amount""
                FROM ""{schema}"".""Payment"" p
                WHERE p.""PaymentDate"" >= @FromDate
                  AND p.""PaymentDate"" <= @ToDate
                GROUP BY p.""PaymentMethod""";

            var param = new { FromDate = fromDate, ToDate = toDate };

            var rawTotal = await _executers.ExecuteSingleReaderAsync<PaymentTotalsQueryResult>(_connStr, totalSql, param);
            var rawMethods = await _executers.ExecuteReaderAsync<PaymentMethodSummary>(_connStr, byMethodSql, param);

            var dto = new PaymentBreakdownDTO
            {
                TotalPayments = (int)(rawTotal?.TotalPayments ?? 0),
                TotalAmount = rawTotal?.TotalAmount ?? 0,
                ByMethod = rawMethods?.ToList() ?? new List<PaymentMethodSummary>(),
                FromDate = fromDate,
                ToDate = toDate
            };

            return BaseResponse<PaymentBreakdownDTO>.Success(dto, ResponseMessages.ReportGenerated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<FrontDeskSummaryDTO>> GetFrontDeskSummaryAsync(DateTime date)
        {
            _logger.LogInformation("Generating {ReportType} report for schema {Schema}", "FrontDesk", Schema);
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);
            var schema = Schema;
            var dateOnly = DateTime.SpecifyKind(date.Date, DateTimeKind.Utc);

            var sql = $@"
                SELECT
                    COUNT(*) FILTER (WHERE DATE(r.""CheckInDate"") = DATE(@Date) AND r.""Status"" IN (1, 2)) AS ""ExpectedArrivals"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 2 AND r.""LastModifiedDate"" IS NOT NULL AND DATE(r.""LastModifiedDate"") = DATE(@Date)) AS ""ActualCheckIns"",
                    COUNT(*) FILTER (WHERE DATE(r.""CheckOutDate"") = DATE(@Date) AND r.""Status"" IN (2, 3)) AS ""ExpectedDepartures"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 3 AND r.""LastModifiedDate"" IS NOT NULL AND DATE(r.""LastModifiedDate"") = DATE(@Date)) AS ""ActualCheckOuts"",
                    COUNT(*) FILTER (WHERE r.""Status"" = 2) AS ""CurrentlyOccupied""
                FROM ""{schema}"".""Reservation"" r
                WHERE r.""IsDeleted"" = false";

            var srSql = $@"
                SELECT COUNT(*) FROM ""{schema}"".""ServiceRequest"" s
                WHERE s.""IsDeleted"" = false AND s.""ServiceRequestState"" = 0";

            var hkSql = $@"
                SELECT COUNT(*) FROM ""{schema}"".""HousekeepingTask"" h
                WHERE h.""IsDeleted"" = false AND h.""State"" = 0
                  AND DATE(h.""ScheduledAt"") = DATE(@Date)";

            var param = new { Date = dateOnly };

            var rawRes = await _executers.ExecuteSingleReaderAsync<FrontDeskQueryResult>(_connStr, sql, param);
            var pendingSr = await _executers.ExecuteSingleReaderAsync<long>(_connStr, srSql, param);
            var pendingHk = await _executers.ExecuteSingleReaderAsync<long>(_connStr, hkSql, param);

            var dto = new FrontDeskSummaryDTO
            {
                Date = dateOnly,
                ExpectedArrivals = (int)(rawRes?.ExpectedArrivals ?? 0),
                ActualCheckIns = (int)(rawRes?.ActualCheckIns ?? 0),
                ExpectedDepartures = (int)(rawRes?.ExpectedDepartures ?? 0),
                ActualCheckOuts = (int)(rawRes?.ActualCheckOuts ?? 0),
                CurrentlyOccupied = (int)(rawRes?.CurrentlyOccupied ?? 0),
                PendingServiceRequests = (int)pendingSr,
                PendingHousekeepingTasks = (int)pendingHk
            };

            return BaseResponse<FrontDeskSummaryDTO>.Success(dto, ResponseMessages.ReportGenerated, ResponseStatusCode.OperationSuccessful);
        }

        public async Task<BaseResponse<ExpenseReportDTO>> GetExpenseReportAsync(DateTime fromDate, DateTime toDate, string? search = null, long? reservationId = null)
        {
            _logger.LogInformation("Generating {ReportType} report for schema {Schema}", "Expenses", Schema);
            fromDate = DateTime.SpecifyKind(fromDate, DateTimeKind.Utc);
            toDate = DateTime.SpecifyKind(toDate, DateTimeKind.Utc);
            var schema = Schema;

            var searchFilter = "";
            if (reservationId.HasValue)
                searchFilter = "AND e.\"ReservationId\" = @ReservationId";
            else if (!string.IsNullOrWhiteSpace(search))
                searchFilter = "AND (g.\"FullName\" ILIKE @Search OR CAST(e.\"ReservationId\" AS text) = @Search)";

            var itemsSql = $@"
                SELECT
                    e.""Id"",
                    e.""ReservationId"",
                    g.""FullName"" AS ""GuestName"",
                    g.""Email"" AS ""GuestEmail"",
                    rm.""Number"" AS ""RoomNumber"",
                    e.""Description"",
                    e.""Category"",
                    e.""Quantity"",
                    e.""UnitPrice"",
                    e.""Amount"",
                    e.""CreatedBy"",
                    e.""CreationDate""
                FROM ""{schema}"".""ReservationExpense"" e
                INNER JOIN ""{schema}"".""Reservation"" r ON e.""ReservationId"" = r.""Id"" AND r.""IsDeleted"" = false
                LEFT JOIN ""{schema}"".""GuestProfile"" g ON r.""GuestId"" = g.""Id""
                LEFT JOIN ""{schema}"".""Room"" rm ON r.""RoomId"" = rm.""Id""
                WHERE e.""IsDeleted"" = false
                  AND e.""CreationDate"" >= @FromDate
                  AND e.""CreationDate"" <= @ToDate
                {searchFilter}
                ORDER BY e.""CreationDate"" DESC";

            var categorySql = $@"
                SELECT
                    COALESCE(e.""Category"", 'Uncategorized') AS ""Category"",
                    COUNT(*) AS ""Count"",
                    COALESCE(SUM(e.""Amount""), 0) AS ""Amount""
                FROM ""{schema}"".""ReservationExpense"" e
                INNER JOIN ""{schema}"".""Reservation"" r ON e.""ReservationId"" = r.""Id"" AND r.""IsDeleted"" = false
                LEFT JOIN ""{schema}"".""GuestProfile"" g ON r.""GuestId"" = g.""Id""
                WHERE e.""IsDeleted"" = false
                  AND e.""CreationDate"" >= @FromDate
                  AND e.""CreationDate"" <= @ToDate
                {searchFilter}
                GROUP BY COALESCE(e.""Category"", 'Uncategorized')
                ORDER BY ""Amount"" DESC";

            // Build params without unused nullables — Npgsql throws when it
            // can't infer a PostgreSQL type for a null anonymous-object property.
            object param;
            if (reservationId.HasValue)
                param = new { FromDate = fromDate, ToDate = toDate, ReservationId = reservationId.Value };
            else if (!string.IsNullOrWhiteSpace(search))
                param = new { FromDate = fromDate, ToDate = toDate, Search = $"%{search}%" };
            else
                param = new { FromDate = fromDate, ToDate = toDate };

            var rawItems = await _executers.ExecuteReaderAsync<ExpenseReportItemQueryResult>(_connStr, itemsSql, param);
            var rawCategories = await _executers.ExecuteReaderAsync<ExpenseCategorySummaryQueryResult>(_connStr, categorySql, param);

            var items = (rawItems ?? Enumerable.Empty<ExpenseReportItemQueryResult>())
                .Select(i => new ExpenseReportItemDTO
                {
                    Id = i.Id,
                    ReservationId = i.ReservationId,
                    GuestName = i.GuestName,
                    GuestEmail = i.GuestEmail,
                    RoomNumber = i.RoomNumber,
                    Description = i.Description,
                    Category = i.Category,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Amount = i.Amount,
                    CreatedBy = i.CreatedBy,
                    CreationDate = i.CreationDate,
                })
                .ToList();

            var categories = (rawCategories ?? Enumerable.Empty<ExpenseCategorySummaryQueryResult>())
                .Select(c => new ExpenseCategorySummary
                {
                    Category = c.Category,
                    Count = c.Count,
                    Amount = c.Amount,
                })
                .ToList();

            var dto = new ExpenseReportDTO
            {
                Items = items,
                TotalItems = items.Count,
                TotalAmount = items.Sum(i => i.Amount),
                ByCategory = categories,
                FromDate = fromDate,
                ToDate = toDate,
            };

            return BaseResponse<ExpenseReportDTO>.Success(dto, ResponseMessages.ReportGenerated, ResponseStatusCode.OperationSuccessful);
        }
    }
}
