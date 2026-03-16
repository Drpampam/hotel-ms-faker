using FluentValidation;
using FluentValidation.AspNetCore;
using FluentValidation.Results;
using hotelier_core_app.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace hotelier_core_app.Core.Interceptors
{
    /// <summary>
    /// Interceptor for handling ASP.NET model validation using FluentValidation.
    /// </summary>
    public class RequestModelValidatorInterceptor : IValidatorInterceptor
    {
        /// <summary>
        /// Called after ASP.NET validation to handle validation errors.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <param name="result">The validation result.</param>
        /// <returns>The validation result, or throws a DataValidationException if invalid.</returns>
        public ValidationResult AfterAspNetValidation(ActionContext actionContext, IValidationContext validationContext, ValidationResult result)
        {
            if (!result.IsValid)
            {
                var validationErrors = new Dictionary<string, string>();
                foreach (var error in result.Errors)
                {
                    validationErrors.Add(error.PropertyName, error.ErrorMessage);
                }
                throw new DataValidationException("One or more request parameters are not valid", validationErrors);
            }
            return result;
        }

        /// <summary>
        /// Called before ASP.NET validation.
        /// </summary>
        /// <param name="actionContext">The action context.</param>
        /// <param name="commonContext">The common validation context.</param>
        /// <returns>The validation context.</returns>
        public IValidationContext BeforeAspNetValidation(ActionContext actionContext, IValidationContext commonContext)
        {
            return commonContext;
        }
    }
}
