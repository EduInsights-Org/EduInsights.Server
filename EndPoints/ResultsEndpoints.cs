using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class ResultsEndpoints
{
    private const string ResultsEndpointsName = "/api/v1/results";

    public static void MapResultsEndpointsName(this WebApplication app)
    {
        var group = app.MapGroup(ResultsEndpointsName).WithTags("EduInsights endpoints");

        group.MapPost("/",
            async (CreateResultRequest request, IResultService resultService) =>
            {
                var result = await resultService.AddResultAsync(request);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapGet("/",
            async (IResultService resultService) =>
            {
                var result = await resultService.GetAllResultsAsync();
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapGet("/grade-distribution",
            async (IResultService resultService, string? instituteId) =>
            {
                var result = await resultService.GetGradeDistribution(instituteId);
                return Results.Json(result, statusCode: result.StatusCode);
            });
    }
}