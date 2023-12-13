using SFA.DAS.EarlyConnect.Infrastructure.OuterApi;
using System;
using System.Net;

namespace SFA.DAS.EarlyConnect.Infrastructure.Extensions
{
    public static class ApiResponseErrorChecking
    {
        public static ApiResponse<T> EnsureSuccessStatusCode<T>(this ApiResponse<T> response)
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response));
            }

            if (!IsSuccessStatusCode(response.StatusCode))
            {
                throw ApiResponseException.Create(response);
            }

            return response;
        }

        public static bool IsSuccessStatusCode(HttpStatusCode statusCode)
            => (int)statusCode >= 200 && (int)statusCode <= 299;
    }
    public class ApiResponseException : Exception
    {
        public HttpStatusCode Status { get; }
        public string Error { get; }

        public static ApiResponseException Create<T>(ApiResponse<T> response)
        {
            return new ApiResponseException(response.StatusCode, response.ErrorContent);
        }

        public ApiResponseException(HttpStatusCode status, string error)
            : base($"HTTP status code did not indicate success: {(int)status} {status}")
        {
            Status = status;
            Error = error;
        }

        public ApiResponseException(HttpStatusCode status, string error, Exception innerException)
            : base($"HTTP status code did not indicate success: {(int)status} {status}", innerException)
        {
            Status = status;
            Error = error;
        }
    }
}