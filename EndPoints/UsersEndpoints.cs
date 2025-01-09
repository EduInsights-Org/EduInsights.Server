using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduInsights.Server.EndPoints;

public static class UsersEndpoints
{
    private const string UserEndpointName = "/api/v1/users";

    public static RouteGroupBuilder MapUsersEndPoints(this WebApplication app)
    {
        var group = app.MapGroup(UserEndpointName).WithTags("EduInsights endpoints");

        group.MapGet("/{id}", async (string id, [FromServices] IUserService userService) =>
        {
            
            var result = await userService.GetUserByIdAsync(id);
            return result is null ? Results.NotFound() : Results.Ok(result.User);
        });

        group.MapGet("/", async ([FromServices] IUserService userService) =>
        {
            var result = await userService.GetAllUsers();
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapPost("/", async ([FromBody] CreateUserRequest createUser, [FromServices] IUserService userService) =>
        {
            await userService.AddUserAsync(createUser);
            return Results.Created();
        });

        group.MapPost("/multi-add",
            async ([FromBody] CreateUserRequest[] request, [FromServices] IUserService userService) =>
            {
                try
                {
                    var response = await userService.AddUsersAsync(request);

                    if (response.Success)
                    {
                        return Results.Created(UserEndpointName, new
                        {
                            message = response.Message,
                            addedUsers = response.SuccessfullyAddedUsers,
                            invalidUsers = response.InvalidUsers,
                            existingUsers = response.ExistingUsers
                        });
                    }

                    return Results.BadRequest(new
                    {
                        message = response.Message,
                        invalidUsers = response.InvalidUsers,
                        existingUsers = response.ExistingUsers
                    });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            });

        return group;
    }
}