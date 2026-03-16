namespace hotelier_core_app.API.Extensions
{
    /// <summary>
    /// Extension methods for registering tenant middleware in the application pipeline.
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        /// <summary>
        /// Registers the <see cref="Middleware.TenantMiddleware"/> in the application's request pipeline.
        /// </summary>
        /// <param name="builder">The application builder.</param>
        /// <returns>The application builder with tenant middleware registered.</returns>
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<Middleware.TenantMiddleware>();
        }
    }
}
