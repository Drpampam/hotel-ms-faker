using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace hotelier_core_app.API.Helpers
{
    /// <summary>
    /// Schema filter for excluding properties from Swagger documentation using the <see cref="SwaggerIgnorePropertyAttribute"/>.
    /// </summary>
    public class SwaggerExcludeFilter : ISchemaFilter
    {
        /// <summary>
        /// Applies the filter to exclude properties marked with <see cref="SwaggerIgnorePropertyAttribute"/> from the schema.
        /// </summary>
        /// <param name="schema">The OpenAPI schema.</param>
        /// <param name="context">The schema filter context.</param>
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null)
            {
                return;
            }

            var excludedProperties = context.Type.GetProperties().Where(t => t.GetCustomAttribute<SwaggerIgnorePropertyAttribute>() != null);

            foreach (var excludedProperty in excludedProperties)
            {
                var propertyToRemove = schema.Properties.Keys.SingleOrDefault(x => string.Equals(x, excludedProperty.Name, StringComparison.OrdinalIgnoreCase));

                if (propertyToRemove != null)
                {
                    schema.Properties.Remove(propertyToRemove);
                }
            }
        }
    }

    /// <summary>
    /// Attribute to mark properties to be excluded from Swagger documentation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class SwaggerIgnorePropertyAttribute : Attribute
    {
    }
}
