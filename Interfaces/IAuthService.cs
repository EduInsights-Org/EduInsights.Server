
using EduInsights.Server.Contracts;

namespace EduInsights.Server.Interfaces;

public interface IAuthService
{
    Task Register(RegisterUserRequest request);
    Task<LoginUserResponse> Login(LoginUserRequest request);
    Task Logout();
    Task<RefreshResponse> Refresh();
}
