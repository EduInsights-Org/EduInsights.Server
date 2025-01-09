using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IBatchService
{
    Task<List<Batch>?> GetBatchesByInstituteIdAsync(string instituteId);

    Task<Batch> AddBatchAsync(CreateBatchRequest batch);

    Task<List<Batch>?> GetAllBatches();

}
