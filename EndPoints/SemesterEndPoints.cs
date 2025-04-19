using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class SemesterEndPoints
{
    private const string SemestersEndpointsName = "/api/v1/semesters";

    public static void MapSemestersEndpointsName(this WebApplication app)
    {
        var group = app.MapGroup(SemestersEndpointsName).WithTags("EduInsights endpoints");

        group.MapPost("/",
            async (CreateSemesterRequest semester, ISemesterService semesterService) =>
            {
                var result = await semesterService.AddSemesterAsync(semester);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapGet("/",
            async (ISemesterService semesterService, string? instituteId = null) =>
            {
                var result = await semesterService.GetAllSemesterAsync(instituteId);
                return Results.Json(result, statusCode: result.StatusCode);
            });
    }
}