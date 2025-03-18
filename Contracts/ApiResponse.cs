using EduInsights.Server.Enums;

namespace EduInsights.Server.Contracts
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public ErrorCode? ErrorCode { get; set; }

        public static ApiResponse<T> SuccessResult(T data, int statusCode = HttpStatusCode.Ok, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Data = data, StatusCode = statusCode, Message = message };
        }

        public static ApiResponse<T> ErrorResult(string message, int statusCode,
            ErrorCode errorCode = Enums.ErrorCode.UnDefinedError)
        {
            return new ApiResponse<T>
                { Success = false, StatusCode = statusCode, Message = message, ErrorCode = errorCode };
        }
    }
}