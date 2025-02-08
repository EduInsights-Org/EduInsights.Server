using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record class LoginUserResponse(
    string RefreshToken,
    User UserInfo
);