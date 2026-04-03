using hotelier_core_app.Core.Exceptions;
using hotelier_core_app.Model.DTOs.Response;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using System.Net;

namespace hotelier_core_app.API.Helpers
{
    /// <summary>
    /// Extension methods for configuring global exception handling middleware.
    /// </summary>
    public static class ExceptionMiddleware
    {
        /// <summary>
        /// Configures the application's exception handler to return a standardized error response.
        /// </summary>
        /// <param name="app">The application builder.</param>
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

                        // BaseException subclasses (e.g. DataValidationException) carry their own status code
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
