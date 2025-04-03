using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface ISubjectService
{
    Task<ApiResponse<List<Subject>>> GetAllSubjectsAsync();

    Task<ApiResponse<Subject>> AddSubjectAsync(CreateSubjectRequest subject);
    
    Task<ApiResponse<bool>> AddSubjectsAsync(CreateSubjectRequest[] subjects);
}