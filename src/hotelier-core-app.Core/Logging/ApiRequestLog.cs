namespace hotelier_core_app.Core.Logging
{
    /// <summary>
    /// Represents a log entry for an API request, including request and response details.
    /// </summary>
    internal class ApiRequestLog
    {
        /// <summary>
        /// The URL of the API request.
        /// </summary>
        public string? RequestUrl { get; set; }

        /// <summary>
        /// The HTTP method used for the request.
        /// </summary>
        public string? HttpMethod { get; set; }

        /// <summary>
        /// The serialized request payload.
        /// </summary>
        public string? Request { get; set; }

        /// <summary>
        /// The HTTP response status code.
        /// </summary>
        public string? HttpResponseStatusCode { get; set; }

        /// <summary>
        /// The serialized response payload.
        /// </summary>
        public string? Response { get; set; }

        /// <summary>
        /// Any exception message encountered during the request.
        /// </summary>
        public string? ExceptionMessage { get; set; }
    }
}
