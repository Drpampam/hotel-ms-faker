using Dapper;
using hotelier_core_app.Core.Enums;
using hotelier_core_app.Migrations;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json;

namespace hotelier_core_app.API.Middleware
{
    public class LicenseEnforcementMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _connStr;
        private readonly ILogger<LicenseEnforcementMiddleware> _logger;

        private static readonly HashSet<string> _exemptPrefixes = new(StringComparer.OrdinalIgnoreCase)
        {
            "/api/v1/activation",
            "/api/v1/user/login",
            "/api/v1/user/refresh-token",
            "/api/v1/user/forgot-password",
            "/api/v1/user/reset-password",
            "/api/v1/subscription",
            "/healthz",
            "/swagger"
        };

        public LicenseEnforcementMiddleware(RequestDelegate next, IConfiguration configuration, ILogger<LicenseEnforcementMiddleware> logger)
        {
            _next = next;
            _connStr = configuration.GetConnectionString("DbConnectionString") ?? string.Empty;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var reqId = context.TraceIdentifier;

            if (_exemptPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                await _next(context);
                return;
            }

            var tenantIdHeader = context.Request.Headers["X-Tenant-Id"].ToString();
            if (!long.TryParse(tenantIdHeader, out var tenantId) || tenantId <= 0)
            {
                await _next(context);
                return;
            }

            try
            {
                using var conn = new NpgsqlConnection(_connStr);
                var tenant = await conn.QueryFirstOrDefaultAsync<TenantLicenseInfo>(
                    @"SELECT ""IsSuspended"", ""SuspendedUntil"", ""SubscriptionEndDate"", ""PlanType""
                      FROM public.""Tenant""
                      WHERE ""Id"" = @Id AND ""IsDeleted"" = false",
                    new { Id = tenantId });

                if (tenant == null)
                {
                    _logger.LogWarning("[LICENSE {ReqId}] tenant {TenantId} not found in public.Tenant — passing through", reqId, tenantId);
                    await _next(context);
                    return;
                }

                var now = DateTime.UtcNow;
                var isSuspended = tenant.IsSuspended
                    || (tenant.SuspendedUntil.HasValue && tenant.SuspendedUntil.Value > now);

                if (isSuspended)
                {
                    _logger.LogWarning("[LICENSE {ReqId}] tenant {TenantId} is suspended (until={SuspendedUntil}) → 403", reqId, tenantId, tenant.SuspendedUntil);
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        status = false,
                        message = "This account has been suspended. Please contact support.",
                        suspendedUntil = tenant.SuspendedUntil
                    }));
                    return;
                }

                var isUnlimited = tenant.PlanType == (int)PlanType.Unlimited;
                var isExpired = !isUnlimited
                    && tenant.SubscriptionEndDate.HasValue
                    && tenant.SubscriptionEndDate.Value < now;

                if (isExpired)
                {
                    _logger.LogWarning("[LICENSE {ReqId}] tenant {TenantId} subscription expired at {ExpiredAt} → 402", reqId, tenantId, tenant.SubscriptionEndDate);
                    context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(new
                    {
                        status = false,
                        message = "Your subscription has expired. Please renew to continue.",
                        expiredAt = tenant.SubscriptionEndDate
                    }));
                    return;
                }
            }
            catch (Exception ex)
            {
                // License check failed (e.g. DB unreachable) — let request through but log the error
                // so we can distinguish "license OK" from "check skipped due to infra failure".
                _logger.LogError(ex, "[LICENSE {ReqId}] license check failed for tenant {TenantId} — passing through: {Message}", reqId, tenantId, ex.Message);
            }

            await _next(context);
        }

        private sealed class TenantLicenseInfo
        {
            public bool IsSuspended { get; init; }
            public DateTime? SuspendedUntil { get; init; }
            public DateTime? SubscriptionEndDate { get; init; }
            public int? PlanType { get; init; }
        }
    }
}
