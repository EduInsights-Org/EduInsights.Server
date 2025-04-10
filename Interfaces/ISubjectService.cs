using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface ISubjectService
{
    Task<ApiResponse<List<Subject>>> GetAllSubjectsAsync();

    Task<ApiResponse<Subject>> AddSubjectAsync(CreateSubjectRequest subject);

    Task<ApiResponse<AddSubjectsResponse>> AddSubjectsAsync(CreateSubjectRequest[] subjects);

    Task<ApiResponse<PaginatedResponse<List<Subject>>>> GetSubjectsAsync(string? instituteId, int page, int pageSize);
}