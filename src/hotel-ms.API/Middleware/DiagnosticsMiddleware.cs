using System.Diagnostics;

namespace hotelier_core_app.API.Middleware
{
    /// <summary>
    /// Logs every request (method, path, tenant header, status, elapsed ms) to stdout.
    /// Keeps the production client response unchanged — purely server-side observability.
    /// </summary>
    public class DiagnosticsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<DiagnosticsMiddleware> _logger;

        public DiagnosticsMiddleware(RequestDelegate next, ILogger<DiagnosticsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();
            var reqId = context.TraceIdentifier;
            var method = context.Request.Method;
            var path = context.Request.Path + context.Request.QueryString;
            var tenant = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "(none)";

            _logger.LogInformation("[REQ  {ReqId}] {Method} {Path} | tenant={Tenant}",
                reqId, method, path, tenant);

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var status = context.Response.StatusCode;
                var level = status >= 500 ? LogLevel.Error
                          : status >= 400 ? LogLevel.Warning
                          : LogLevel.Information;

                // Auth runs inside _next, so we can read the resolved identity here.
                var user = context.User?.Identity?.IsAuthenticated == true
                    ? (context.User.Identity.Name ?? context.User.FindFirst("sub")?.Value ?? "(unknown)")
                    : "(unauthenticated)";

                var roles = context.User?.Claims
                    .Where(c => c.Type == System.Security.Claims.ClaimTypes.Role)
                    .Select(c => c.Value)
                    .ToList();
                var roleStr = roles?.Count > 0 ? string.Join(",", roles) : "(none)";

                _logger.Log(level,
                    "[RESP {ReqId}] {Method} {Path} → {Status} ({Elapsed}ms) | tenant={Tenant} | user={User} | roles={Roles}",
                    reqId, method, path, status, sw.ElapsedMilliseconds, tenant, user, roleStr);
            }
        }
    }
}
