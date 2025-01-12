using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IUserService
{
    Task<ApiResponse<User>> AddUserAsync(CreateUserRequest createUserRequest);
    Task<ApiResponse<AddUsersResponse>> AddUsersAsync(CreateUserRequest[] createUsersRequest);
    Task<ApiResponse<User>> GetUserByIdAsync(string id);
    Task<ApiResponse<List<User>>> GetAllUsers();
    Task<ApiResponse<User>> FindUserByUserName(string userName);
}