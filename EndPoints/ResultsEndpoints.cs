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
            async (IResultService resultService, string? instituteId, string? batchId = null) =>
            {
                var result = await resultService.GetAllResultsAsync(instituteId, batchId);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapGet("/grade-distribution",
            async (IResultService resultService, string instituteId) =>
            {
                var result = await resultService.GetGradeDistribution(instituteId);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapGet("/students-gpa",
            async (IResultService resultService, string? instituteId, string? batchId) =>
            {
                var result = await resultService.CalculateAllStudentGPAsAsync(instituteId, batchId);
                return Results.Json(result, statusCode: result.StatusCode);
            });

        group.MapGet("/average-gpas",
            async (IResultService resultService, string? instituteId) =>
            {
                var result = await resultService.GetBatchAverageGPAsAsync(instituteId);
                return Results.Json(result, statusCode: result.StatusCode);
            });
    }
}