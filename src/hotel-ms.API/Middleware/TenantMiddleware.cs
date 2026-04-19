using hotelier_core_app.Migrations;
using System.Globalization;

namespace hotelier_core_app.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        // These paths always use the public schema — they deal with users/auth
        // which are global, not per-tenant.
        private static readonly string[] _publicSchemaPaths = new[]
        {
            "/api/v1/user/login",
            "/api/v1/user/refresh-token",
            "/api/v1/user/forgot-password",
            "/api/v1/user/reset-password",
            "/api/v1/user/create-user",
            "/api/v1/user/get-all-users",
            "/api/v1/user/get-user-by-email",
            "/api/v1/activation",
            "/api/v1/subscription",
            "/healthz",
            "/swagger",
        };

        public TenantMiddleware(RequestDelegate next) => _next = next;

        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
        {
            var path = context.Request.Path.Value ?? string.Empty;

            // Auth / global paths always hit the public schema.
            if (_publicSchemaPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                tenantProvider.SetSchema("public");
                await _next(context);
                return;
            }

            var tenantId = context.Request.Headers["X-Tenant-Id"].ToString();

            // No header → public schema fallback.
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                tenantProvider.SetSchema("public");
                await _next(context);
                return;
            }

            // Accept only positive numeric tenant IDs.
            if (!long.TryParse(tenantId, NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedTenantId) || normalizedTenantId <= 0)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid X-Tenant-Id header.");
                return;
            }

            tenantProvider.SetSchema($"tenant_{normalizedTenantId}");
            await _next(context);
        }
    }
}
