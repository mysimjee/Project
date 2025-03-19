using System.Text.Json.Serialization;

namespace user_management.Helpers
{
    public class ApiResponse<T>
    {
        public int StatusCode { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        [JsonConstructor]
        public ApiResponse(int statusCode, string message, T data = default)
        {
            StatusCode = statusCode;
            Message = message;
            Data = data;
            Type = GetResponseType(statusCode);
        }

        private string GetResponseType(int statusCode)
        {
            return statusCode switch
            {
                >= 200 and < 300 => "Success",
                >= 400 and < 500 => "Client Error",
                >= 500 => "Server Error",
                _ => "Unknown"
            };
        }

        // Success responses
        public static ApiResponse<T> Success(T data, string message = "Request successful") =>
            new ApiResponse<T>(200, message, data);

        public static ApiResponse<T> Created(T data, string message = "Resource created successfully") =>
            new ApiResponse<T>(201, message, data);

        // Error responses
        public static ApiResponse<T> NotFound(string message = "Resource not found") =>
            new ApiResponse<T>(404, message);

        public static ApiResponse<T> BadRequest(string message = "Invalid request") =>
            new ApiResponse<T>(400, message);

        public static ApiResponse<T> ServerError(string message = "Internal server error") =>
            new ApiResponse<T>(500, message);

        public static ApiResponse<T> Error(string message = "An error occurred") =>
            new ApiResponse<T>(500, message);

        public static ApiResponse<T> Error(Exception ex, string message = "An error occurred") =>
            new ApiResponse<T>(500, $"{message}: {ex.Message}");


        public static ApiResponse<T> Conflict(string message) =>
            new ApiResponse<T>(409, message);

        public static ApiResponse<T> Unauthorized(string message = "Unauthorized") =>
            new ApiResponse<T>(401, message);
    }
}
