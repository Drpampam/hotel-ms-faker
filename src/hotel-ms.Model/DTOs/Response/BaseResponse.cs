namespace hotelier_core_app.Model.DTOs.Response
{
    /// <summary>
    /// Base response class for API responses.
    /// </summary>
    public class BaseResponse
    {
        /// <summary>
        /// Gets or sets the status of the response.
        /// </summary>
        public bool Status { get; set; }

        /// <summary>
        /// Gets or sets the status code of the response.
        /// </summary>
        public string? StatusCode { get; set; }

        /// <summary>
        /// Gets or sets the message associated with the response.
        /// </summary>
        public string? Message { get; set; }

        /// <summary>
        /// Creates a successful response.
        /// </summary>
        /// <param name="message">The success message.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns>A new <see cref="BaseResponse"/> instance representing success.</returns>
        public static BaseResponse Success(string message = "", string statusCode = "")
        {
            return new BaseResponse()
            {
                Status = true,
                StatusCode = statusCode ?? "00",
                Message = message ?? "Operation successful!"
            };
        }

        /// <summary>
        /// Creates a failure response.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns>A new <see cref="BaseResponse"/> instance representing failure.</returns>
        public static BaseResponse Failure(string message = "", string statusCode = "")
        {
            return new BaseResponse()
            {
                StatusCode = statusCode ?? "06",
                Message = message ?? "Sorry, there was an error processing your request."
            };
        }
    }

    /// <summary>
    /// Generic base response class for API responses with data.
    /// </summary>
    /// <typeparam name="T">The type of the data returned in the response.</typeparam>
    public class BaseResponse<T> : BaseResponse
    {
        /// <summary>
        /// Gets or sets the data returned in the response.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        /// Creates a successful response with data.
        /// </summary>
        /// <param name="data">The data to return.</param>
        /// <param name="message">The success message.</param>
        /// <param name="statusCode">The status code.</param>
        /// <returns>A new <see cref="BaseResponse{T}"/> instance representing success.</returns>
        public static BaseResponse<T> Success(T data, string message = "", string statusCode = "")
        {
            return new BaseResponse<T>()
            {
                Status = true,
                StatusCode = statusCode ?? "00",
                Message = message ?? "Operation successful!",
                Data = data
            };
        }

        public static BaseResponse<T> Failure(T data, string message = "", string statusCode = "")
        {
            return new BaseResponse<T>()
            {
                StatusCode = statusCode ?? "06",
                Message = message ?? "Sorry, there was an error processing your request.",
                Data = data
            };
        }
    }

    public class PageBaseResponse<T> : BaseResponse
    {
        public T? Data { get; set; }
        public int? DataCount { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPageCount { get; set; }
        public bool HasPreviousPage
        {
            get
            {
                return (PageNumber > 1);
            }
        }
        public bool HasNextPage
        {
            get
            {
                return (PageNumber < TotalPageCount);
            }
        }

        public static PageBaseResponse<T> Success(T data, string message = "", int? count = 1, string statusCode = "")
        {
            return new PageBaseResponse<T>()
            {
                Status = true,
                StatusCode = statusCode ?? "00",
                Message = message ?? "Operation successful!",
                Data = data,
                DataCount = count
            };
        }

        public static PageBaseResponse<T> Failure(T data, string message = "", int? count = 0, string statusCode = "")
        {
            return new PageBaseResponse<T>()
            {
                StatusCode = statusCode ?? "06",
                Message = message ?? "Sorry, there was an error processing your request.",
                Data = data,
                DataCount = count
            };
        }

        public static PageBaseResponse<T> Success(T data, string message = "", int count = 1, string statusCode = "", int totalPageCount = 1, int pageNumber = 1, int pageSize = 1)
        {
            return new PageBaseResponse<T>()
            {
                Status = true,
                StatusCode = statusCode ?? "00",
                Message = message ?? "Operation successful!",
                Data = data,
                DataCount = count,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPageCount = totalPageCount
            };
        }
    }
}
