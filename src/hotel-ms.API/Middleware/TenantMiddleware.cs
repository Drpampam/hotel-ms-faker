using hotelier_core_app.Migrations;
using System.Globalization;

namespace hotelier_core_app.API.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        // These paths always use the public schema — users, roles, and UserRoles
        // all live in the public schema; tenant schemas are for hotel business data only.
        private static readonly string[] _publicSchemaPaths = new[]
        {
            "/api/v1/user/login",
            "/api/v1/user/refresh-token",
            "/api/v1/user/forgot-password",
            "/api/v1/user/reset-password",
            "/api/v1/user/create-user",
            "/api/v1/user/get-all-users",
            "/api/v1/user/get-users",
            "/api/v1/user/get-user-by-email",
            "/api/v1/user/update-user",
            "/api/v1/user/reassign-role",
            "/api/v1/user/activate-user",
            "/api/v1/user/deactivate-user",
            "/api/v1/user/admin-change-password",
            "/api/v1/user/change-temp-password",
            "/api/v1/user/change-shift",
            "/api/v1/user/delete-user",
            "/api/v1/user/get-assigned-modules",
            "/api/v1/role",
            "/api/v1/activation",
            "/api/v1/subscription",
            "/healthz",
            "/swagger",
        };

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
        {
            var path = context.Request.Path.Value ?? string.Empty;
            var reqId = context.TraceIdentifier;

            // Auth / global paths always hit the public schema.
            if (_publicSchemaPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
            {
                tenantProvider.SetSchema("public");
                _logger.LogDebug("[TENANT {ReqId}] {Path} → schema=public (public-path match)", reqId, path);
                await _next(context);
                return;
            }

            var tenantId = context.Request.Headers["X-Tenant-Id"].ToString();

            // No header → public schema fallback.
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                tenantProvider.SetSchema("public");
                _logger.LogWarning("[TENANT {ReqId}] {Path} → schema=public (no X-Tenant-Id header)", reqId, path);
                await _next(context);
                return;
            }

            // Accept only positive numeric tenant IDs.
            if (!long.TryParse(tenantId, NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedTenantId) || normalizedTenantId <= 0)
            {
                _logger.LogWarning("[TENANT {ReqId}] {Path} → 400 bad tenant id '{TenantId}'", reqId, path, tenantId);
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid X-Tenant-Id header.");
                return;
            }

            var schema = $"tenant_{normalizedTenantId}";
            tenantProvider.SetSchema(schema);
            _logger.LogInformation("[TENANT {ReqId}] {Path} → schema={Schema}", reqId, path, schema);
            await _next(context);
        }
    }
}
