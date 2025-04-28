using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using MongoDB.Driver;

namespace EduInsights.Server.Interfaces;

public interface ISubjectService
{
    Task<ApiResponse<List<Subject>>> GetAllSubjectsAsync(string? instituteId);

    Task<ApiResponse<Subject>> AddSubjectAsync(CreateSubjectRequest subject);

    Task<ApiResponse<AddSubjectsResponse>> AddSubjectsAsync(CreateSubjectRequest[] subjects);

    Task<ApiResponse<PaginatedResponse<List<Subject>>>> GetSubjectsAsync(string? instituteId, int page, int pageSize);
    
    Task<ApiResponse<Subject>> GetSubjectByFilterAsync(FilterDefinition<Subject>? filter = null);
}