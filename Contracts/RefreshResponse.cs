using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record RefreshResponse
{
    public required string AccessToken { get; set; }
    public required string UserId { get; set; }
}