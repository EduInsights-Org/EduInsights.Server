using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class BatchService(IMongoDatabase database) : IBatchService
{
    private readonly IMongoCollection<Batch> _batchesCollection = database.GetCollection<Batch>("batches");

    public async Task<List<Batch>?> GetBatchesByInstituteIdAsync(string instituteId)
    {
        try
        {
            return await _batchesCollection.Find(b => b.InstituteId == instituteId).ToListAsync();
        }
        catch
        {
            throw new Exception("An error occurred while retrieving the batches.");
        }
    }

    public async Task<Batch> AddBatchAsync(CreateBatchRequest request)
    {
        try
        {
            var batch = new Batch
            {
                Name = request.Name,
                InstituteId = request.InstituteId
            };
            await _batchesCollection.InsertOneAsync(batch);
            return batch;
        }
        catch
        {
            throw new Exception("An error occurred while creating the batch.");
        }
    }

    public async Task<List<Batch>?> GetAllBatches()
    {
        try
        {
            return await _batchesCollection.Find(_ => true).ToListAsync();
        }
        catch
        {
            throw new Exception("An error occurred while retrieving the batches.");
        }
    }
}