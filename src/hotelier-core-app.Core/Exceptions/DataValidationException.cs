using System.Net;
using System.Runtime.Serialization;

namespace hotelier_core_app.Core.Exceptions
{
    /// <summary>
    /// Exception representing a data validation error with HTTP 400 status code.
    /// </summary>
    internal class DataValidationException : BaseException
    {
        /// <summary>
        /// Gets the HTTP status code for a data validation error (BadRequest).
        /// </summary>
        public override HttpStatusCode StatusCode => HttpStatusCode.BadRequest;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataValidationException"/> class with default message and optional details.
        /// </summary>
        /// <param name="details">Additional details about the validation error.</param>
        public DataValidationException(object? details = null)
            : base("Sorry, we couldn't make that happen. Please, try again.", details)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataValidationException"/> class with a custom message and optional details.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="details">Additional details about the validation error.</param>
        public DataValidationException(string message, object? details = null)
            : base(message, details)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataValidationException"/> class with a custom message, inner exception, and optional details.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        /// <param name="details">Additional details about the validation error.</param>
        public DataValidationException(string message, Exception inner, object? details = null)
            : base(message, inner, details)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataValidationException"/> class for serialization.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        /// <param name="details">Additional details about the validation error.</param>
        protected DataValidationException(SerializationInfo info, StreamingContext context, object? details = null)
            : base(info, context, details)
        {
        }
    }
}
