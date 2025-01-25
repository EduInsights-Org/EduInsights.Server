using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IUserService
{
    Task<ApiResponse<User>> AddUserAsync(CreateUserRequest createUserRequest);
    Task<ApiResponse<AddUsersResponse>> AddUsersAndStudentsAsync(CreateUserRequest[] createUsersRequest);
    Task<ApiResponse<User>> GetUserByIdAsync(string id);
    Task<ApiResponse<List<User>>> GetAllUsers();
    Task<ApiResponse<List<GetUserWithStudentResponse>>> GetUsers(string? instituteId = null, string? batchId = null);
    Task<ApiResponse<User>> FindUserByUserName(string userName);
}