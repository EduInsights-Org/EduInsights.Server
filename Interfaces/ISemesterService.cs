using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using MongoDB.Driver;

namespace EduInsights.Server.Interfaces;

public interface ISemesterService
{
    Task<ApiResponse<Semester>> AddSemesterAsync(CreateSemesterRequest semester);
    Task<ApiResponse<List<Semester>>> GetAllSemesterAsync(string? instituteId);
    Task<ApiResponse<Semester>> GetSemesterByFilterAsync(FilterDefinition<Semester>? filter = null);
}