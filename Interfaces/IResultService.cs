using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IResultService
{
    Task<ApiResponse<Result>> AddResultAsync(CreateResultRequest result);

    Task<ApiResponse<List<GetResultResponse>>> GetAllResultsAsync(string instituteId, string? batchId);
    Task<ApiResponse<GetGradeDistribution>> GetGradeDistribution(string? instituteId);

    Task<ApiResponse<List<StudentGpaResponse>>> CalculateAllStudentGPAsAsync(string? instituteId,
        string? batchId = null);

    Task<ApiResponse<List<BatchGpaResponse>>> GetBatchAverageGPAsAsync(string? instituteId);
}