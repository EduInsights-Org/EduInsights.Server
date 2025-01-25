using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduInsights.Server.EndPoints;

public static class UsersEndpoints
{
    private const string UserEndpointName = "/api/v1/users";

    public static void MapUsersEndPoints(this WebApplication app)
    {
        var group = app.MapGroup(UserEndpointName).WithTags("EduInsights endpoints");

        group.MapGet("/{id}", async (string id, [FromServices] IUserService userService) =>
        {
            var result = await userService.GetUserByIdAsync(id);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/", async ([FromServices] IUserService userService) =>
        {
            var result = await userService.GetAllUsers();
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapPost("/", async ([FromBody] CreateUserRequest createUser, [FromServices] IUserService userService) =>
        {
            var response = await userService.AddUserAsync(createUser);
            return Results.Json(response, statusCode: response.StatusCode);
        });

        group.MapPost("/multi-add",
            async ([FromBody] CreateUserRequest[] request, [FromServices] IUserService userService) =>
            {
                var response = await userService.AddUsersAndStudentsAsync(request);
                return Results.Json(response, statusCode: response.StatusCode);
            });
    }
}