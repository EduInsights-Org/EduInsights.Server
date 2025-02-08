using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IUserService
{
    Task<ApiResponse<User>> AddUserAsync(CreateUserRequest createUserRequest);
    Task<ApiResponse<AddUsersResponse>> AddUsersAndStudentsAsync(CreateUserRequest[] createUsersRequest);
    Task<ApiResponse<User>> GetUserByIdAsync(string id);

    Task<ApiResponse<List<User>>> GetAllUsers();

    Task<ApiResponse<GetRoleDistributionResponse>> GetRoleDistribution(string? instituteId);

    Task<ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>> GetUsers(
        string? instituteId, string? batchId, int page, int pageSize);

    Task<ApiResponse<User>> FindUserByUserName(string userName);
}