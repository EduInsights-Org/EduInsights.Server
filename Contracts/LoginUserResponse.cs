using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record LoginUserResponse(
    string RefreshToken,
    User UserInfo
);