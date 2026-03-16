using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Core.Helpers
{
    /// <summary>
    /// Validation attribute to ensure a date is not in the past.
    /// </summary>
    public class PastDateValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates that the value is a date not in the past.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A ValidationResult indicating success or failure.</returns>
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is DateTime dt)
            {
                if (dt.Date >= DateTime.Today.Date)
                    return ValidationResult.Success ?? new ValidationResult("Past date entry not allowed");
                else
                    return new ValidationResult("Past date entry not allowed");
            }
            return new ValidationResult("Invalid date supplied");
        }
    }
}
