namespace EduInsights.Server.Contracts
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
        public int StatusCode { get; set; }

        public static ApiResponse<T> SuccessResult(T data, int statusCode = 200, string message = "Success")
        {
            return new ApiResponse<T> { Success = true, Data = data, StatusCode = statusCode, Message = message };
        }

        public static ApiResponse<T> ErrorResult(string message, int statusCode)
        {
            return new ApiResponse<T> { Success = false, StatusCode = statusCode, Message = message };
        }
    }
}