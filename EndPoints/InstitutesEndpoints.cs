using EduInsights.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduInsights.Server.EndPoints;

public static class InstitutesEndpoints
{
    private const string InstituteEndpointName = "/api/v1/institutes";

    public static RouteGroupBuilder MapInstitutesEndPoints(this WebApplication app)
    {
        var group = app.MapGroup(InstituteEndpointName).WithTags("EduInsights endpoints");

        group.MapGet("/", async ([FromServices] IInstituteService instituteService) =>
        {
            var result = await instituteService.GetAllInstitutes();
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/{userId}", async (string userId, [FromServices] IInstituteService instituteService) =>
        {
            var result = await instituteService.GetInstituteByUserIdAsync(userId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        return group;
    }
}
