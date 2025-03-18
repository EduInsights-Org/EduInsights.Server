namespace EduInsights.Server.Contracts;

public record VerifyEmailRequest
{
    public required string Code { get; set; }
    public required string Email { get; set; }
}