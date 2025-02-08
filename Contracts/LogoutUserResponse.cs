using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record LogoutUserResponse(
    string RefreshToken,
    string UserName
);