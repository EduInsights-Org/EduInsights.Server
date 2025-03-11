using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class InstitutesEndpoints
{
    private const string InstituteEndpointName = "/api/v1/institutes";

    public static void MapInstitutesEndPoints(this WebApplication app)
    {
        var group = app.MapGroup(InstituteEndpointName).WithTags("EduInsights endpoints");

        group.MapGet("/", async (IInstituteService instituteService) =>
        {
            var result = await instituteService.GetAllInstitutes();
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/{userId}", async (string userId, IInstituteService instituteService) =>
        {
            var result = await instituteService.GetInstituteByUserIdAsync(userId);
            return Results.Json(result, statusCode: result.StatusCode);
        });
    }
}