using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using MongoDB.Driver;

namespace EduInsights.Server.Interfaces;

public interface IBatchService
{
    Task<ApiResponse<List<Batch>>> GetBatchesByInstituteIdAsync(string instituteId);

    Task<ApiResponse<Batch>> AddBatchAsync(CreateBatchRequest batch);

    Task<ApiResponse<List<Batch>>> GetAllBatches();
    
    Task<ApiResponse<Batch>> BatchByFilterAsync(FilterDefinition<Batch>? filter = null);


}
