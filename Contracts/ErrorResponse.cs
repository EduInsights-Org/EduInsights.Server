namespace EduInsights.Server.Contracts;

public record class ErrorResponse(
    string Title,
    int StatusCode,
    string Message
);