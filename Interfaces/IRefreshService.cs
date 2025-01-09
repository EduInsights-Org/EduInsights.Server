
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IRefreshService
{
    public string GenerateAccessToken(string userId, string userRole);
    Task<RefreshToken> GenerateRefreshToken(string userId);
    Task RevokeRefreshToken(string token);
    Task<RefreshToken> GetRefreshToken(string token);
    Task<RefreshToken?> GetRefreshTokenByUserId(string userId);
    Task<bool> ValidateRefreshToken(string token);
}
