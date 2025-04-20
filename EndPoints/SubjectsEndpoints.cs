using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class SubjectsEndpoints
{
    private const string SubjectsEndpointsName = "/api/v1/subjects";

    public static void MapSubjectsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(SubjectsEndpointsName).WithTags("EduInsights endpoints");

        group.MapGet("/",
            async (ISubjectService subjectService, string? instituteId = null) =>
            {
                var result = await subjectService.GetAllSubjectsAsync(instituteId);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapPost("/", async (CreateSubjectRequest subject, ISubjectService subjectService) =>
        {
            var result = await subjectService.AddSubjectAsync(subject);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapPost("/multi-add", async (CreateSubjectRequest[] subjects, ISubjectService subjectService) =>
        {
            var result = await subjectService.AddSubjectsAsync(subjects);
            return Results.Json(result, statusCode: result.StatusCode);
        });
    }
}