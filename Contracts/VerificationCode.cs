namespace EduInsights.Server.Contracts;

public class VerificationCode
{
    public required string Code { get; set; }
    public DateTime ExpiresAt { get; set; }
}