namespace EduInsights.Server.Contracts;

public record LoginUserRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}