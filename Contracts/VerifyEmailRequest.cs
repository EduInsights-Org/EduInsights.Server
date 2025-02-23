namespace EduInsights.Server.Contracts;

public class VerifyEmailRequest
{
    public required string Code { get; set; }
    public required string Email { get; set; }
}