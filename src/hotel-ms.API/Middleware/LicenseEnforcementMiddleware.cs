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

        public LicenseEnforcementMiddleware(RequestDelegate next, IConfiguration configuration)
        {
            _next = next;
            _connStr = configuration.GetConnectionString("DbConnectionString") ?? string.Empty;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var path = context.Request.Path.Value ?? string.Empty;

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
                    await _next(context);
                    return;
                }

                var now = DateTime.UtcNow;
                var isSuspended = tenant.IsSuspended
                    || (tenant.SuspendedUntil.HasValue && tenant.SuspendedUntil.Value > now);

                if (isSuspended)
                {
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
            catch
            {
                // If the license check itself fails (e.g. DB unreachable), let the request
                // through so a transient infrastructure error doesn't lock out all users.
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
