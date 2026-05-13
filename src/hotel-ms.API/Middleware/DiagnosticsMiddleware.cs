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
            var method = context.Request.Method;
            var path = context.Request.Path + context.Request.QueryString;
            var tenant = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? "(none)";
            var user = context.User?.Identity?.Name ?? "(unauthenticated)";

            _logger.LogInformation("[REQ ] {Method} {Path} | tenant={Tenant} | user={User}",
                method, path, tenant, user);

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

                _logger.Log(level,
                    "[RESP] {Method} {Path} → {Status} ({Elapsed}ms) | tenant={Tenant}",
                    method, path, status, sw.ElapsedMilliseconds, tenant);
            }
        }
    }
}
