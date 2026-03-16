using hotelier_core_app.Model.DTOs.Response;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json;

namespace hotelier_core_app.API.Helpers
{
    /// <summary>
    /// Represents a validation error for a specific field.
    /// </summary>
    public class ValidationError
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        /// <summary>
        /// The name of the field with the validation error.
        /// </summary>
        public string Field { get; }

        /// <summary>
        /// The validation error message.
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationError"/> class.
        /// </summary>
        /// <param name="field">The field name.</param>
        /// <param name="message">The error message.</param>
        public ValidationError(string field, string message)
        {
            Field = field != string.Empty ? field : string.Empty;
            Message = message;
        }
    }

    /// <summary>
    /// Represents the result of a model validation, including all validation errors.
    /// </summary>
    public class ValidationResultModel : BaseResponse
    {
        /// <summary>
        /// The list of validation errors.
        /// </summary>
        public List<ValidationError> Data { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationResultModel"/> class.
        /// </summary>
        /// <param name="modelState">The model state dictionary containing validation errors.</param>
        public ValidationResultModel(ModelStateDictionary modelState)
        {
            Message = "Validation Failed";
            Data = modelState.Keys
                .Where(key => modelState[key]?.Errors != null)
                .SelectMany(key => modelState[key].Errors?.Select(x => new ValidationError(key, x.ErrorMessage)) ?? Enumerable.Empty<ValidationError>())
                .ToList();
        }
    }

    /// <summary>
    /// Represents an HTTP 400 response for a failed model validation.
    /// </summary>
    public class ValidationFailedResult : ObjectResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ValidationFailedResult"/> class.
        /// </summary>
        /// <param name="modelState">The model state dictionary containing validation errors.</param>
        public ValidationFailedResult(ModelStateDictionary modelState)
            : base(new ValidationResultModel(modelState))
        {
            StatusCode = StatusCodes.Status400BadRequest;
        }
    }
}
