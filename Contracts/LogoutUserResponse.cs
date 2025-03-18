namespace EduInsights.Server.Contracts;

public record LogoutUserResponse
{
    public required string RefreshToken { get; set; }
    public required string UserName { get; set; }
}