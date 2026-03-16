using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace hotelier_core_app.Core.Helpers
{
    /// <summary>
    /// Validation attribute to ensure a date string is in YYYY-MM-DD format.
    /// </summary>
    public class DateValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates that the value is a date string in YYYY-MM-DD format.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A ValidationResult indicating success or failure.</returns>
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            var dateString = Convert.ToString(value);
            if (string.IsNullOrWhiteSpace(dateString))
                return ValidationResult.Success ?? new ValidationResult("Sorry! Invalid date format entered. Use YYYY-MM-DD format only"); ;

            bool ok = DateTime.TryParseExact(
               dateString,
               "yyyy-MM-dd",
               CultureInfo.InvariantCulture,
               DateTimeStyles.None,
               out _
            );

            if (ok)
                return ValidationResult.Success ?? new ValidationResult("Sorry! Invalid date format entered. Use YYYY-MM-DD format only"); ;
            return new ValidationResult("Sorry! Invalid date format entered. Use YYYY-MM-DD format only");
        }
    }
}
