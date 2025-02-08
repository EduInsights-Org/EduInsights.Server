using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduInsights.Server.EndPoints;

public static class BatchesEndpoints
{
    private const string BatchesEndpointsName = "/api/v1/batches";

    public static void MapBatchesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(BatchesEndpointsName).WithTags("EduInsights endpoints");

        group.MapGet("/{instituteId}", async (string instituteId, [FromServices] IBatchService batchService) =>
        {
            var result = await batchService.GetBatchesByInstituteIdAsync(instituteId);
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapGet("/", async ([FromServices] IBatchService batchService) =>
        {
            var result = await batchService.GetAllBatches();
            return Results.Json(result, statusCode: result.StatusCode);
        });

        group.MapPost("/", async ([FromBody] CreateBatchRequest request, [FromServices] IBatchService batchService) =>
        {
            var result = await batchService.AddBatchAsync(request);
            return Results.Json(result, statusCode: result.StatusCode);
        });
    }
}