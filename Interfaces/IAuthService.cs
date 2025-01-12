using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IAuthService
{
    Task<ApiResponse<User>> Register(RegisterUserRequest request);
    Task<ApiResponse<LoginUserResponse>> Login(LoginUserRequest request);
    Task<ApiResponse<LogoutUserResponse>> Logout();
    Task<ApiResponse<RefreshResponse>> Refresh();
}