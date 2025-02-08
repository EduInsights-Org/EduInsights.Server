using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IRefreshService
{
    public ApiResponse<string> GenerateAccessToken(string userId, string userRole);
    Task<ApiResponse<RefreshToken>> GenerateRefreshToken(string userId);
    Task<ApiResponse<string>> RevokeRefreshToken(string token);
    Task<ApiResponse<RefreshToken>> GetRefreshToken(string token);
    Task<ApiResponse<RefreshToken>> GetRefreshTokenByUserId(string userId);
    Task<ApiResponse<bool>> ValidateRefreshToken(string token);
}