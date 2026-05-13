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

                    if (contextFeature != null)
                    {
                        var ex = contextFeature.Error;

                        // Log every unhandled exception so Render/server logs capture the real cause.
                        var logger = context.RequestServices
                            .GetService<ILoggerFactory>()
                            ?.CreateLogger("GlobalExceptionHandler");
                        logger?.LogError(ex,
                            "Unhandled exception [{Type}] on {Method} {Path}: {Message}",
                            ex.GetType().Name,
                            context.Request.Method,
                            context.Request.Path,
                            ex.Message);

                        if (ex is BaseException baseEx)
                        {
                            context.Response.StatusCode = (int)baseEx.StatusCode;
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new BaseResponse
                            {
                                Status = false,
                                Message = baseEx.Message
                            }));
                        }
                        else
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new BaseResponse
                            {
                                Status = false,
                                Message = "An unexpected error occurred. Please try again."
                            }));
                        }
                    }
                });
            });
        }
    }
}
