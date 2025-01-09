using EduInsights.Server.Entities;

namespace EduInsights.Server.Contracts;

public record class RefreshResponse(
    string AccessToken,
    User UserInfo
);