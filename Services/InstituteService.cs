using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Enums;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class InstituteService(IMongoDatabase database, ILogger<InstituteService> logger) : IInstituteService
{
    private readonly IMongoCollection<Institute>
        _institutesCollection = database.GetCollection<Institute>("institutes");

    public async Task<ApiResponse<Institute>> GetInstituteByUserIdAsync(string id)
    {
        try
        {
            var institute = await _institutesCollection.Find(i => i.Id == id).FirstOrDefaultAsync();
            return institute == null
                ? ApiResponse<Institute>.ErrorResult("Institute not found", HttpStatusCode.NotFound)
                : ApiResponse<Institute>.SuccessResult(institute);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching institute: {ex.Message}", ex.Message);
            return ApiResponse<Institute>.ErrorResult("Error when fetching institute", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<Institute>> AddInstituteAsync(Institute institute)
    {
        try
        {
            await _institutesCollection.InsertOneAsync(institute);
            return ApiResponse<Institute>.SuccessResult(institute);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding institute: {ex.Message}", ex.Message);
            return ApiResponse<Institute>.ErrorResult("Error when adding Institute", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<Institute>>> GetAllInstitutes()
    {
        try
        {
            var institute = await _institutesCollection.Find(_ => true).ToListAsync();
            return institute is null
                ? ApiResponse<List<Institute>>.ErrorResult("Institutes not found", HttpStatusCode.NotFound)
                : ApiResponse<List<Institute>>.SuccessResult(institute);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching Institutes: {ex.Message}", ex.Message);
            return ApiResponse<List<Institute>>.ErrorResult("Error when fetching Institutes", HttpStatusCode.InternalServerError);
        }
    }
}