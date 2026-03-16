using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace hotelier_core_app.API.Helpers
{
    /// <summary>
    /// Operation filter for adding security headers to Swagger documentation.
    /// </summary>
    public class SwaggerHeaderFilter : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to add bearer token security requirements to the operation.
        /// </summary>
        /// <param name="operation">The OpenAPI operation.</param>
        /// <param name="context">The operation filter context.</param>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Security ??= new List<OpenApiSecurityRequirement>();

            OpenApiSecurityScheme openApiSecurityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearer"
                }
            };
            operation.Security.Add(new OpenApiSecurityRequirement { [openApiSecurityScheme] = new List<string>() });
        }
    }
}
