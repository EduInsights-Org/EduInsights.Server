using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class UserService(
    IMongoDatabase database,
    IStudentService studentService,
    ILogger<UserService> logger)
    : IUserService
{
    private readonly IMongoCollection<User> _userCollection = database.GetCollection<User>("users");

    public async Task<ApiResponse<User>> AddUserAsync(CreateUserRequest createUser)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(createUser.Password) ||
                string.IsNullOrWhiteSpace(createUser.FirstName) ||
                string.IsNullOrWhiteSpace(createUser.LastName) ||
                string.IsNullOrWhiteSpace(createUser.UserName) ||
                string.IsNullOrWhiteSpace(createUser.Email))
            {
                return ApiResponse<User>.ErrorResult("Invalid data found", 400);
            }

            var existingUser =
                await _userCollection.Find(u => u.UserName == createUser.UserName).FirstOrDefaultAsync();
            if (existingUser != null)
                return ApiResponse<User>.ErrorResult("User already existed", 400);

            var user = new User
            {
                FirstName = createUser.FirstName,
                LastName = createUser.LastName,
                UserName = createUser.UserName,
                Email = createUser.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUser.UserName),
                InstituteId = createUser.InstituteId,
                Role = createUser.Role,
                CreatedAt = DateTime.UtcNow,
            };
            await _userCollection.InsertOneAsync(user);

            if (createUser.Role != UserRole.Student) return ApiResponse<User>.SuccessResult(user);
            if (string.IsNullOrWhiteSpace(createUser.IndexNumber) ||
                string.IsNullOrWhiteSpace(createUser.BatchId) ||
                string.IsNullOrWhiteSpace(createUser.InstituteId))
            {
                return ApiResponse<User>.ErrorResult("Invalid data found", 400);
            }

            var student = new Student
            {
                IndexNumber = createUser.IndexNumber,
                UserId = user.Id,
                BatchId = createUser.BatchId,
            };
            var addStudentResult = await studentService.AddStudentAsync(student);
            return !addStudentResult.Success
                ? ApiResponse<User>.ErrorResult("Error when adding students", 500)
                : ApiResponse<User>.SuccessResult(user, 200, "User added successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when Adding user");
            return ApiResponse<User>.ErrorResult("Error when Adding user", 500);
        }
    }

    public async Task<ApiResponse<AddUsersResponse>> AddUsersAndStudentsAsync(CreateUserRequest[] createUsersRequest)
    {
        var invalidUsers = new List<string>();
        var existingUsers = new List<string>();
        var successfullyAddedUsers = new List<string>();
        var usersToInsert = new List<User>();

        try
        {
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

                var studentsToInsert = (from s in createUsersRequest
                        where s.Role == UserRole.Student
                        select new Student
                        {
                            IndexNumber = s.IndexNumber!,
                            BatchId = s.BatchId,
                            UserId = usersToInsert.Find(u => u.UserName == s.UserName)!.Id
                        })
                    .ToList();


                //update student collection if role is student
                if (studentsToInsert.Count > 0)
                {
                    var addUsersResult = await studentService.AddStudentsAsync(studentsToInsert);
                    if (!addUsersResult.Success)
                        return ApiResponse<AddUsersResponse>.ErrorResult("Error when adding students", 500);
                }
            }

            var addUsersResponse = new AddUsersResponse
            {
                Success = true,
                AddedUsers = successfullyAddedUsers,
                InvalidUsers = invalidUsers,
                ExistingUsers = existingUsers,
                Message =
                    $"Successfully added {successfullyAddedUsers.Count} users. {invalidUsers.Count} invalid users. {existingUsers.Count} users already existed."
            };
            return ApiResponse<AddUsersResponse>.SuccessResult(addUsersResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when Adding users");
            return ApiResponse<AddUsersResponse>.ErrorResult("Error when adding users", 500);
        }
    }

    public async Task<ApiResponse<User>> GetUserByIdAsync(string id)
    {
        try
        {
            var user = await _userCollection.Find(u => u.Id == id).FirstOrDefaultAsync();
            return user is null
                ? ApiResponse<User>.ErrorResult("User not found", 404)
                : ApiResponse<User>.SuccessResult(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting user");
            return ApiResponse<User>.ErrorResult("Error when getting user", 500);
        }
    }

    public async Task<ApiResponse<List<User>>> GetAllUsers()
    {
        try
        {
            var users = await _userCollection.Find(_ => true).ToListAsync();
            return users is null
                ? ApiResponse<List<User>>.ErrorResult("Users not found", 404)
                : ApiResponse<List<User>>.SuccessResult(users);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting users");
            return ApiResponse<List<User>>.ErrorResult("Error when getting users", 500);
        }
    }

    public async Task<ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>> GetUsers(
        string? instituteId, string? batchId, int page, int pageSize)
    {
        try
        {
            if (!string.IsNullOrEmpty(batchId) && string.IsNullOrEmpty(instituteId))
            {
                return ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>.ErrorResult(
                    "Institute ID must be provided when filtering by Batch ID", 400);
            }

            // Build the filter based on provided parameters
            var filter = Builders<User>.Filter.Empty;

            if (!string.IsNullOrEmpty(instituteId))
            {
                filter &= Builders<User>.Filter.Eq(u => u.InstituteId, instituteId);
            }

            if (!string.IsNullOrEmpty(batchId))
            {
                var studentFilter = Builders<Student>.Filter.Eq(s => s.BatchId, batchId);
                var students = (await studentService.GetStudentsByFilterAsync(studentFilter)).Data;
                if (students is null)
                    return ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>.ErrorResult(
                        "No students found", 404);

                var userIds = students!.Select(s => s.UserId).ToList();
                filter &= Builders<User>.Filter.In(u => u.Id, userIds);
            }

            // Pagination logic
            var totalRecords = await _userCollection.CountDocumentsAsync(filter);
            var userList = await _userCollection
                .Find(filter)
                .ToListAsync();

            if (userList.Count == 0)
            {
                return ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>.ErrorResult(
                    "No users found for the given criteria", 404);
            }

            // Map users to response with student details
            var usersWithStudentDetails = new List<GetUserWithStudentResponse>();
            foreach (var user in userList)
            {
                var student = await studentService.GetStudentByUserIdAsync(user.Id);
                usersWithStudentDetails.Add(new GetUserWithStudentResponse
                {
                    UserName = user.UserName,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Email = user.Email,
                    Role = user.Role,
                    Id = user.Id,
                    IndexNumber = student.Data?.IndexNumber,
                });
            }

            usersWithStudentDetails = usersWithStudentDetails
                .OrderBy(user => UserRole.GetRolePriority(user.Role))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var paginatedResponse = new PaginatedResponse<List<GetUserWithStudentResponse>>(
                Data: usersWithStudentDetails,
                TotalRecords: totalRecords,
                CurrentPage: page,
                PageSize: pageSize
            );

            return ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>.SuccessResult(paginatedResponse);
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Invalid format for institute ID.");
            return ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>.ErrorResult(
                "Invalid ID format.", 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting users");
            return ApiResponse<PaginatedResponse<List<GetUserWithStudentResponse>>>.ErrorResult(
                "Error when getting users", 500);
        }
    }


    public async Task<ApiResponse<GetRoleDistributionResponse>> GetRoleDistribution(string? instituteId)
    {
        try
        {
            if (string.IsNullOrEmpty(instituteId))
                return ApiResponse<GetRoleDistributionResponse>.ErrorResult(
                    "Institute ID cannot be null or empty.", 400);

            var users = await _userCollection
                .Find(user => user.InstituteId == instituteId)
                .Project(user => new { user.Role })
                .ToListAsync();

            if (users.Count == 0)
                return ApiResponse<GetRoleDistributionResponse>.ErrorResult(
                    "No users found for the given Institute", 404);

            var roleCounts = new GetRoleDistributionResponse();

            foreach (var user in users)
            {
                switch (user.Role)
                {
                    case UserRole.SuperAdmin:
                        roleCounts.SuperAdmin++;
                        break;
                    case UserRole.Admin:
                        roleCounts.Admin++;
                        break;
                    case UserRole.DataEntry:
                        roleCounts.DataEntry++;
                        break;
                    case UserRole.Student:
                        roleCounts.Student++;
                        break;
                }
            }

            return ApiResponse<GetRoleDistributionResponse>.SuccessResult(roleCounts);
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Invalid format for institute ID.");
            return ApiResponse<GetRoleDistributionResponse>.ErrorResult(
                "Invalid institute ID format.", 400);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting users.");
            return ApiResponse<GetRoleDistributionResponse>.ErrorResult(
                "Error when getting users.", 500);
        }
    }

    public async Task<ApiResponse<User>> FindUserByUserName(string userName)
    {
        try
        {
            var user = await _userCollection.Find(u => u.UserName == userName).FirstOrDefaultAsync();
            return user is null
                ? ApiResponse<User>.ErrorResult("User not found", 404)
                : ApiResponse<User>.SuccessResult(user);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting user");
            return ApiResponse<User>.ErrorResult("Error when getting user", 500);
        }
    }
}