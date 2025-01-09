using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IInstituteService
{
    Task<Institute?> GetInstituteByUserIdAsync(string id);

    Task AddInstituteAsync(Institute institute);

    Task<List<Institute>?> GetAllInstitutes();

}
