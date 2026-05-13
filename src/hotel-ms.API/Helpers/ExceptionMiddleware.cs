using hotelier_core_app.Core.Exceptions;
using hotelier_core_app.Model.DTOs.Response;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Net;

namespace hotelier_core_app.API.Helpers
{
    public static class ExceptionMiddleware
    {
        public static void ConfigureExceptionHandler(this IApplicationBuilder app)
        {
            app.UseExceptionHandler(appError =>
            {
                appError.Run(async context =>
                {
                    var contextFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    context.Response.ContentType = "application/json";

                    if (contextFeature == null) return;

                    var ex = contextFeature.Error;

                    var logger = context.RequestServices
                        .GetService<ILoggerFactory>()
                        ?.CreateLogger("GlobalExceptionHandler");

                    // Log full exception + stack trace so Render logs capture every detail.
                    logger?.LogError(ex,
                        "[500] {ExType} on {Method} {Path} | {Message}",
                        ex.GetType().FullName,
                        context.Request.Method,
                        context.Request.Path,
                        ex.Message);

                    if (ex is BaseException baseEx)
                    {
                        context.Response.StatusCode = (int)baseEx.StatusCode;
                        await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                        {
                            Status = false,
                            Message = baseEx.Message
                        }));
                        return;
                    }

                    // In non-production environments (or when DIAG_ERRORS env var is set)
                    // expose the real exception so it is visible in the API response.
                    var expose = !app.ApplicationServices
                                        .GetRequiredService<IWebHostEnvironment>()
                                        .IsProduction()
                                  || Environment.GetEnvironmentVariable("DIAG_ERRORS") == "1";

                    await context.Response.WriteAsync(JsonConvert.SerializeObject(new
                    {
                        Status = false,
                        Message = "An unexpected error occurred. Please try again.",
                        // Visible when DIAG_ERRORS=1 is set on the server (Render env vars).
                        Debug = expose ? new
                        {
                            ExceptionType = ex.GetType().FullName,
                            ex.Message,
                            InnerException = ex.InnerException?.Message,
                            StackTrace = ex.StackTrace
                        } : null
                    }));
                });
            });
        }
    }
}
