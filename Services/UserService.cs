using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class UserService(IMongoDatabase database, IStudentService studentService) : IUserService
{
    private readonly IMongoCollection<User> _userCollection = database.GetCollection<User>("users");

    public async Task<AddUserResponse> AddUserAsync(CreateUserRequest createUser)
    {
        try
        {
            var user = new User
            {
                FirstName = createUser.FirstName,
                LastName = createUser.LastName,
                UserName = createUser.LastName,
                Role = createUser.Role,
                PasswordHash = createUser.Password,
                CreatedAt = DateTime.Now
            };
            await _userCollection.InsertOneAsync(user);
            return new AddUserResponse("User registered successfully", true);
        }
        catch (Exception ex)
        {
            return new AddUserResponse($"Failed to register user: {ex.Message}", false);
        }
    }


    public async Task<AddUsersResponse> AddUsersAsync(CreateUserRequest[] createUsersRequest)
    {
        var invalidUsers = new List<string>();
        var existingUsers = new List<string>();
        var successfullyAddedUsers = new List<string>();

        try
        {
            var usersToInsert = new List<User>();

            foreach (var request in createUsersRequest)
            {
                if (string.IsNullOrWhiteSpace(request.Password) ||
                    string.IsNullOrWhiteSpace(request.FirstName) ||
                    string.IsNullOrWhiteSpace(request.LastName) ||
                    string.IsNullOrWhiteSpace(request.UserName) ||
                    string.IsNullOrWhiteSpace(request.Email) ||
                    string.IsNullOrWhiteSpace(request.InstituteId))
                {
                    var invalidUser = "Unknown user";
                    if (request.LastName.Length > 0)
                    {
                        invalidUser = request.LastName;
                    }

                    if (request.FirstName.Length > 0) invalidUser = request.FirstName;
                    if (request.Email.Length > 0) invalidUser = request.Email;
                    if (request.UserName.Length > 0) invalidUser = request.UserName;
                    invalidUsers.Add(invalidUser);
                    continue;
                }

                var existingUser =
                    await _userCollection.Find(u => u.UserName == request.UserName).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    existingUsers.Add(request.UserName);
                    continue;
                }

                var user = new User
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    UserName = request.UserName,
                    Email = request.Email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.UserName),
                    InstituteId = request.InstituteId,
                    Role = request.Role,
                    CreatedAt = DateTime.UtcNow,
                };
                usersToInsert.Add(user);
            }

            if (usersToInsert.Count > 0)
            {
                await _userCollection.InsertManyAsync(usersToInsert);
                successfullyAddedUsers.AddRange(usersToInsert.Select(u => u.UserName));
            }

            var studentsToInsert = (from s in createUsersRequest
                    where s.Role == "student"
                    select new Student
                    {
                        IndexNumber = s.IndexNumber!,
                        UserId = usersToInsert.Find(u => u.UserName == s.UserName)!.Id
                    })
                .ToList();

            //update student collection if role is student
            if (studentsToInsert.Count > 0)
                await studentService.AddStudentsAsync(studentsToInsert);

            return new AddUsersResponse
            {
                Success = true,
                SuccessfullyAddedUsers = successfullyAddedUsers,
                InvalidUsers = invalidUsers,
                ExistingUsers = existingUsers,
                Message =
                    $"Successfully added {successfullyAddedUsers.Count} users. {invalidUsers.Count} invalid users. {existingUsers.Count} users already existed."
            };
        }
        catch (Exception ex)
        {
            return new AddUsersResponse
            {
                Success = false,
                SuccessfullyAddedUsers = successfullyAddedUsers,
                InvalidUsers = invalidUsers,
                ExistingUsers = existingUsers,
                Message = $"Error when adding users: {ex.Message}"
            };
        }
    }

    public async Task<GetUserResponse?> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            return new GetUserResponse(user, true, "User found successfully");
        }
        catch
        {
            throw new Exception("An error occurred while retrieving the user info.");
        }
    }

    public async Task<List<User>?> GetAllUsers()
    {
        return await _userCollection.Find(_ => true).ToListAsync();
    }

    public async Task<User?> FindUserByUserName(string userName)
    {
        return await _userCollection.Find(u => u.UserName == userName).FirstOrDefaultAsync();
    }
}