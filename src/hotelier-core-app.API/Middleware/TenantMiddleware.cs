using hotelier_core_app.Migrations;
using System.Globalization;

namespace hotelier_core_app.API.Middleware
{
    /// <summary>
    /// Middleware for resolving and setting the tenant schema based on the incoming request.
    /// </summary>
    public class TenantMiddleware
    {
        /// <summary>
        /// The next middleware in the pipeline.
        /// </summary>
        private readonly RequestDelegate _next;

        /// <summary>
        /// Initializes a new instance of the <see cref="TenantMiddleware"/> class.
        /// </summary>
        /// <param name="next">The next middleware in the pipeline.</param>
        public TenantMiddleware(RequestDelegate next) => _next = next;

        /// <summary>
        /// Invokes the middleware to resolve the tenant and set the schema for the request.
        /// </summary>
        /// <param name="context">The HTTP context.</param>
        /// <param name="tenantProvider">The tenant provider to set the schema.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task InvokeAsync(HttpContext context, ITenantProvider tenantProvider)
        {
            var tenantId = context.Request.Headers["X-Tenant-Id"].ToString();

            // Missing header falls back to public schema.
            if (string.IsNullOrWhiteSpace(tenantId))
            {
                tenantProvider.SetSchema("public");
                await _next(context);
                return;
            }

            // Accept only positive numeric tenant IDs and normalize formatting.
            if (!long.TryParse(tenantId, NumberStyles.None, CultureInfo.InvariantCulture, out var normalizedTenantId) || normalizedTenantId <= 0)
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                await context.Response.WriteAsync("Invalid X-Tenant-Id header.");
                return;
            }

            var schema = $"tenant_{normalizedTenantId}";
            tenantProvider.SetSchema(schema);
            await _next(context);
        }
    }
}
