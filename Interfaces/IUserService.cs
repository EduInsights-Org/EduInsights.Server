
using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IUserService
{
    Task<AddUserResponse> AddUserAsync(CreateUserRequest createUserRequest);
    Task<AddUsersResponse> AddUsersAsync(CreateUserRequest[] createUsersRequest);
    Task<GetUserResponse?> GetUserByIdAsync(string id);
    Task<List<User>?> GetAllUsers();
    Task<User?> FindUserByUserName(string userName);
}
