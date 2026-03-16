using System.ComponentModel.DataAnnotations;

namespace hotelier_core_app.Core.Helpers
{
    /// <summary>
    /// Validation attribute to ensure a list of strings contains only valid enum values.
    /// </summary>
    public class EnumListValidationAttribute : ValidationAttribute
    {
        private readonly Type _enumType;
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumListValidationAttribute"/> class.
        /// </summary>
        /// <param name="enumType">The enum type to validate against.</param>
        public EnumListValidationAttribute(Type enumType)
        {
            if (enumType == null || !enumType.IsEnum)
            {
                throw new ArgumentNullException(nameof(enumType), "The type must be an enum type");
            }
            _enumType = enumType;
        }

        /// <summary>
        /// Validates that the value is a list of strings containing only valid enum values.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <param name="validationContext">The validation context.</param>
        /// <returns>A ValidationResult indicating success or failure.</returns>
        protected override ValidationResult IsValid(object? value, ValidationContext validationContext)
        {
            if (value is IEnumerable<string> enumNames)
            {
                var invalidNames = enumNames.Where(enumName => !Enum.IsDefined(_enumType, enumName)).ToList();

                if (invalidNames.Count != 0)
                {
                    var invalidNamesString = string.Join(", ", invalidNames);
                    return new ValidationResult($"{invalidNamesString} are not valid values in {_enumType.Name}");
                }

                return ValidationResult.Success ?? new ValidationResult("Invalid input data type.");
            }

            return new ValidationResult("Invalid input data type. Expected an enumerable of strings.");
        }
    }
}
