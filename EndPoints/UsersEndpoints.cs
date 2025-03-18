using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class UsersEndpoints
{
    private const string UserEndpointName = "/api/v1/users";

    public static void MapUsersEndPoints(this WebApplication app)
    {
        var group = app.MapGroup(UserEndpointName).WithTags("EduInsights endpoints")
            // .RequireAuthorization()
            ;

        group.MapGet("/{id}", async (string id, IUserService userService) =>
        {
            var result = await userService.GetUserByIdAsync(id);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/",
            async (IUserService userService, string? instituteId = null,
                string? batchId = null, int page = 1, int pageSize = 10) =>
            {
                var result = await userService.GetUsers(instituteId, batchId, page, pageSize);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapGet("/role-distribution",
            async (IUserService userService, string? instituteId) =>
            {
                var result = await userService.GetRoleDistribution(instituteId);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapPatch("/{userId}",
            async (string userId, IUserService userService,
                UpdateUserRequest updateUserRequest) =>
            {
                var result = await userService.UpdateUserAsync(userId, updateUserRequest);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapPost("/", async (CreateUserRequest createUser, IUserService userService) =>
        {
            var response = await userService.AddUserAsync(createUser);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/multi-add",
            async (CreateUserRequest[] request, IUserService userService) =>
            {
                var response = await userService.AddUsersAndStudentsAsync(request);
                return Results.Json(response, statusCode: response.StatusCode);
            });

        group.MapDelete("/{userId}", async (string userId, IUserService userService) =>
        {
            var response = await userService.DeleteUserAsync(userId);
            return Results.Json(response, statusCode: response.StatusCode);
        });
    }
}