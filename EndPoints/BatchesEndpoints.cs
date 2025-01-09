using EduInsights.Server.Contracts;
using EduInsights.Server.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EduInsights.Server.EndPoints;

public static class BatchesEndpoints
{
    private const string BatchesEndpointsName = "/api/v1/batches";
    private const string GetBatchEndPointName = "GetBatch";

    public static void MapBatchesEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(BatchesEndpointsName).WithTags("EduInsights endpoints");

        group.MapGet("/{instituteId}", async (string instituteId, [FromServices] IBatchService batchService) =>
        {
            var result = await batchService.GetBatchesByInstituteIdAsync(instituteId);
            return result is null ? Results.NotFound() : Results.Ok(result);
        });

        group.MapGet("/", async ([FromServices] IBatchService batchService) =>
        {
            var result = await batchService.GetAllBatches();
            return result is null ? Results.NotFound() : Results.Ok(result);
        }).WithName(GetBatchEndPointName);

        group.MapPost("/", async ([FromBody] CreateBatchRequest request, [FromServices] IBatchService batchService) =>
        {
            var batch = await batchService.AddBatchAsync(request);
            return Results.CreatedAtRoute(GetBatchEndPointName, new { id = batch.Id }, batch);
        });
    }
}