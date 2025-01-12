using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IInstituteService
{
    Task<ApiResponse<Institute>> GetInstituteByUserIdAsync(string id);

    Task<ApiResponse<Institute>> AddInstituteAsync(Institute institute);

    Task<ApiResponse<List<Institute>>> GetAllInstitutes();
}