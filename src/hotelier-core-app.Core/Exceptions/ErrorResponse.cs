namespace hotelier_core_app.Core.Exceptions
{
    /// <summary>
    /// Represents a standardized error response returned by the API.
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        /// Indicates the status of the response (always false for errors).
        /// </summary>
        public bool Status => false;

        /// <summary>
        /// The error message.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Additional data related to the error.
        /// </summary>
        public object? Data { get; set; }
    }
}
