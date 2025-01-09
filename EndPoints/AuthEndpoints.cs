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
                if (string.IsNullOrWhiteSpace(request.FirstName)
                    || string.IsNullOrWhiteSpace(request.LastName)
                    || string.IsNullOrWhiteSpace(request.UserName)
                    || string.IsNullOrWhiteSpace(request.Password)
                    || string.IsNullOrWhiteSpace(request.InstituteName)
                   ) return Results.BadRequest("Missing Fields required");

                try
                {
                    await authService.Register(request);
                    return Results.Created(AuthEndpointName, new { message = "User registered successfully." });
                }
                catch (Exception ex)
                {
                    return Results.BadRequest(new { message = ex.Message });
                }
            });

        group.MapPost("/login", async ([FromBody] LoginUserRequest request, [FromServices] IAuthService authService) =>
        {
            var result = await authService.Login(request);
            return Results.Ok(new { refreshToken = result.RefreshToken, userInfo = result.User });
        });

        group.MapGet("/refresh", async ([FromServices] IAuthService authService) =>
        {
            var result = await authService.Refresh();
            return Results.Ok(new { accessToken = result.AccessToken, UserId = result.UserInfo.Id });
        });

        group.MapGet("/logout", async ([FromServices] IAuthService authService) =>
        {
            await authService.Logout();
            return Results.Ok("User logged out successfully.");
        });
    }
}