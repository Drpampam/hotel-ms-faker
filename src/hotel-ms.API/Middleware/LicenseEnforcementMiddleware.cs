using hotelier_core_app.Migrations;
using hotelier_core_app.Model.Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace hotelier_core_app.API.Middleware
{
    public class LicenseEnforcementMiddleware
    {
        private readonly RequestDelegate _next;

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

        public LicenseEnforcementMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, AppDbContext db, ITenantProvider tenantProvider)
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

            // Read tenant from public schema
            var previousSchema = tenantProvider.GetSchema();
            tenantProvider.SetSchema("public");

            var tenant = await db.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == tenantId && !t.IsDeleted);

            tenantProvider.SetSchema(previousSchema);

            if (tenant == null)
            {
                await _next(context);
                return;
            }

            var isUnlimited = tenant.PlanType == Core.Enums.PlanType.Unlimited;
            var isExpired = !isUnlimited
                && tenant.SubscriptionEndDate.HasValue
                && tenant.SubscriptionEndDate.Value < DateTime.UtcNow;

            if (isExpired)
            {
                context.Response.StatusCode = StatusCodes.Status402PaymentRequired;
                context.Response.ContentType = "application/json";
                var payload = JsonSerializer.Serialize(new
                {
                    status = false,
                    message = "Your subscription has expired. Please renew to continue.",
                    expiredAt = tenant.SubscriptionEndDate
                });
                await context.Response.WriteAsync(payload);
                return;
            }

            await _next(context);
        }
    }
}
