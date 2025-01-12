using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduInsights.Server.EndPoints;

public static class AuthEndpoints
{
    private const string AuthEndpointName = "/api/v1/auth";

    public static void MapAuthEndPoints(this WebApplication app)
    {
        var group = app.MapGroup(AuthEndpointName).WithTags("EduInsights endpoints");

        group.MapPost("/register",
            async ([FromBody] RegisterUserRequest request, [FromServices] IAuthService authService) =>
            {
                var result = await authService.Register(request);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapPost("/login", async ([FromBody] LoginUserRequest request, [FromServices] IAuthService authService) =>
        {
            var result = await authService.Login(request);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/refresh", async ([FromServices] IAuthService authService) =>
        {
            var result = await authService.Refresh();
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/logout", async ([FromServices] IAuthService authService) =>
        {
            var result = await authService.Logout();
            return Results.Json(result, statusCode: result.StatusCode);
        });
    }
}