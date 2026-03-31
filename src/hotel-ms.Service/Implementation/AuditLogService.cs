using hotelier_core_app.Core.Constants;
using hotelier_core_app.Core.Helpers.Interface;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Model.DTOs.Request;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;

namespace hotelier_core_app.Service.Implementation
{
    public class AuditLogService : IAuditLogService
    {
        private readonly IDBQueryRepository<AuditLog> _auditLogQueryRepository;
        private readonly IUtility _utility;

        public AuditLogService(IDBQueryRepository<AuditLog> auditLogQueryRepository, IUtility utility)
        {
            _auditLogQueryRepository = auditLogQueryRepository;
            _utility = utility;
        }

        public async Task<PageBaseResponse<List<AuditLogResponseDTO>>> GetAuditLogsAsync(GetAuditLogsInputDTO input)
        {
            var all = await _auditLogQueryRepository.GetByAsync(a =>
                (input.PerformerEmail == null || a.PerformerEmail == input.PerformerEmail) &&
                (input.Action == null || a.Action == input.Action) &&
                (!input.FromDate.HasValue || a.DatePerformed >= input.FromDate.Value) &&
                (!input.ToDate.HasValue || a.DatePerformed <= input.ToDate.Value));

            var ordered = all.OrderByDescending(a => a.DatePerformed);
            var paginated = _utility.Paginate(ordered, input.PageNumber, input.PageSize);

            var response = paginated.Select(a => new AuditLogResponseDTO
            {
                Id = a.Id,
                Action = a.Action,
                PerformedBy = a.PerformedBy,
                PerformerEmail = a.PerformerEmail,
                PerformedAgainst = a.PerformedAgainst,
                IpAddress = a.IpAddress,
                DatePerformed = a.DatePerformed
            }).ToList();

            return PageBaseResponse<List<AuditLogResponseDTO>>.Success(response, ResponseMessages.OperationSuccessful,
                count: response.Count, totalPageCount: all.Count(), pageSize: input.PageSize, pageNumber: input.PageNumber);
        }
    }
}
