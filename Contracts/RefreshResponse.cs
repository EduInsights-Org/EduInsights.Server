using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record RefreshResponse(
    string AccessToken,
    string UserId
);