using System.Net;
using System.Runtime.Serialization;

namespace hotelier_core_app.Core.Exceptions
{
    /// <summary>
    /// Abstract base class for custom exceptions with HTTP status code and error details.
    /// </summary>
    public abstract class BaseException : Exception
    {
        /// <summary>
        /// Gets the HTTP status code associated with the exception.
        /// </summary>
        public abstract HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Additional details about the exception.
        /// </summary>
        public object? Details { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseException"/> class with a message and optional details.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="details">Additional details about the exception.</param>
        protected BaseException(string message, object? details = null)
            : base(message)
        {
            Details = details;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseException"/> class with a message, inner exception, and optional details.
        /// </summary>
        /// <param name="message">The exception message.</param>
        /// <param name="inner">The inner exception.</param>
        /// <param name="details">Additional details about the exception.</param>
        protected BaseException(string message, Exception inner, object? details = null)
            : base(message, inner)
        {
            Details = details;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BaseException"/> class for serialization.
        /// </summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        /// <param name="details">Additional details about the exception.</param>
        protected BaseException(SerializationInfo info, StreamingContext context, object? details = null)
            : base(info, context)
        {
            Details = details;
        }

        /// <summary>
        /// Creates an <see cref="ErrorResponse"/> object from the exception.
        /// </summary>
        /// <returns>An <see cref="ErrorResponse"/> containing the exception message and details.</returns>
        public ErrorResponse CreateErrorResponse()
        {
            return new ErrorResponse
            {
                Message = Message,
                Data = Details
            };
        }
    }
}
