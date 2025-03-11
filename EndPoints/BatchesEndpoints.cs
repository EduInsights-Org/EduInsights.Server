using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class BatchesEndpoints
{
    private const string BatchesEndpointsName = "/api/v1/batches";

    public static void MapBatchesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(BatchesEndpointsName).WithTags("EduInsights endpoints");

        group.MapGet("/{instituteId}", async (string instituteId, IBatchService batchService) =>
        {
            var result = await batchService.GetBatchesByInstituteIdAsync(instituteId);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/", async (IBatchService batchService) =>
        {
            var result = await batchService.GetAllBatches();
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapPost("/", async (CreateBatchRequest request, IBatchService batchService) =>
        {
            var result = await batchService.AddBatchAsync(request);
            return Results.Json(result, statusCode: result.StatusCode);
        });
    }
}