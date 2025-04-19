using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Enums;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;
using Exception = System.Exception;

namespace EduInsights.Server.Services;

public class SemesterService(IMongoDatabase database, ILogger<BatchService> logger) : ISemesterService
{
    private readonly IMongoCollection<Semester> _semester = database.GetCollection<Semester>("semester");

    public async Task<ApiResponse<Semester>> AddSemesterAsync(CreateSemesterRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Year)
                || string.IsNullOrWhiteSpace(request.Sem)
               ) return ApiResponse<Semester>.ErrorResult("Validation error.", HttpStatusCode.BadRequest);

            var semester = new Semester
            {
                Year = request.Year,
                Sem = request.Sem,
                InstituteId = request.InstituteId
            };
            await _semester.InsertOneAsync(semester);
            return ApiResponse<Semester>.SuccessResult(semester);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding Semester: {ex.Message}", ex.Message);
            return ApiResponse<Semester>.ErrorResult("Error when adding Semester", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<Semester>>> GetAllSemesterAsync(string? instituteId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(instituteId))
                return ApiResponse<List<Semester>>.ErrorResult(
                    "Institute ID must be provided when getting semesters", HttpStatusCode.BadRequest);

            var semesters = await _semester.Find(s => s.InstituteId == instituteId).ToListAsync();
            return semesters is null
                ? ApiResponse<List<Semester>>.ErrorResult("Semesters not found.", HttpStatusCode.NotFound)
                : ApiResponse<List<Semester>>.SuccessResult(semesters);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching semester: {ex.Message}", ex.Message);
            return ApiResponse<List<Semester>>.ErrorResult("Error when fetching Semesterd",
                HttpStatusCode.InternalServerError);
        }
    }
}