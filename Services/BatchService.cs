using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class BatchService(IMongoDatabase database, ILogger<BatchService> logger) : IBatchService
{
    private readonly IMongoCollection<Batch> _batchesCollection = database.GetCollection<Batch>("batches");

    public async Task<ApiResponse<List<Batch>>> GetBatchesByInstituteIdAsync(string instituteId)
    {
        try
        {
            var batches = await _batchesCollection.Find(b => b.InstituteId == instituteId).ToListAsync();
            return batches is null
                ? ApiResponse<List<Batch>>.ErrorResult("Batches not found for provided institute ID.", 404)
                : ApiResponse<List<Batch>>.SuccessResult(batches);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching Batches: {ex.Message}", ex.Message);
            return ApiResponse<List<Batch>>.ErrorResult("Error when fetching Batches", 500);
        }
    }

    public async Task<ApiResponse<Batch>> AddBatchAsync(CreateBatchRequest request)
    {
        try
        {
            var batch = new Batch
            {
                Name = request.Name,
                InstituteId = request.InstituteId
            };
            await _batchesCollection.InsertOneAsync(batch);
            return ApiResponse<Batch>.SuccessResult(batch);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding Batch: {ex.Message}", ex.Message);
            return ApiResponse<Batch>.ErrorResult("Error when adding Batch", 500);
        }
    }

    public async Task<ApiResponse<List<Batch>>> GetAllBatches()
    {
        try
        {
            var batches = await _batchesCollection.Find(_ => true).ToListAsync();
            return batches is null
                ? ApiResponse<List<Batch>>.ErrorResult("Batches not found.", 404)
                : ApiResponse<List<Batch>>.SuccessResult(batches);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching Batch: {ex.Message}", ex.Message);
            return ApiResponse<List<Batch>>.ErrorResult("Error when fetching Batches", 500);
        }
    }
}