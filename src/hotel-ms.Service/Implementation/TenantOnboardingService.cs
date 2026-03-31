using hotelier_core_app.Core.Constants;
using hotelier_core_app.Domain.Queries.Interface;
using hotelier_core_app.Migrations;
using hotelier_core_app.Model.DTOs.Response;
using hotelier_core_app.Model.Entities;
using hotelier_core_app.Service.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace hotelier_core_app.Service.Implementation
{
    public class TenantOnboardingService : ITenantOnboardingService
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;
        private readonly IDBQueryRepository<Tenant> _tenantQueryRepository;
        private readonly ILogger<TenantOnboardingService> _logger;

        public TenantOnboardingService(
            AppDbContext context,
            ITenantProvider tenantProvider,
            IDBQueryRepository<Tenant> tenantQueryRepository,
            ILogger<TenantOnboardingService> logger)
        {
            _context = context;
            _tenantProvider = tenantProvider;
            _tenantQueryRepository = tenantQueryRepository;
            _logger = logger;
        }

        public async Task<BaseResponse<TenantProvisionResponseDTO>> ProvisionTenantAsync(long tenantId, string performedBy)
        {
            var tenant = await _tenantQueryRepository.FindAsync(tenantId);
            if (tenant == null)
                return BaseResponse<TenantProvisionResponseDTO>.Failure(
                    new TenantProvisionResponseDTO(),
                    ResponseMessages.TenantNotExisting,
                    ResponseStatusCode.NoRecordFound);

            var schema = $"tenant_{tenantId}";
            _tenantProvider.SetSchema(schema);

            _logger.LogInformation("Provisioning tenant {TenantId} with schema {Schema} by {PerformedBy}", tenantId, schema, performedBy);

            await _context.Database.MigrateAsync();

            _logger.LogInformation("Tenant {TenantId} provisioned successfully with schema {Schema}", tenantId, schema);

            return BaseResponse<TenantProvisionResponseDTO>.Success(
                new TenantProvisionResponseDTO
                {
                    TenantId = tenantId,
                    Schema = schema,
                    IsProvisioned = true,
                    Message = $"Tenant schema '{schema}' provisioned successfully"
                },
                ResponseMessages.OperationSuccessful,
                ResponseStatusCode.OperationSuccessful);
        }
    }
}
