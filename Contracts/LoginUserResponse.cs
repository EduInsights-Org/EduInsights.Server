using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record LoginUserResponse
{
    public required string RefreshToken { get; set; }
    public required User UserInfo { get; set; }
};