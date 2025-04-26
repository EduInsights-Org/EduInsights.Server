using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IResultService
{
    Task<ApiResponse<Result>> AddResultAsync(CreateResultRequest result);

    Task<ApiResponse<List<GetResultResponse>>> GetAllResultsAsync();
    Task<ApiResponse<GetGradeDistribution>> GetGradeDistribution(string? instituteId);
}