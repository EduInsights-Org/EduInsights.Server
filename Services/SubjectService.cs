using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Enums;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class SubjectService(IMongoDatabase database, ILogger<BatchService> logger) : ISubjectService
{
    private readonly IMongoCollection<Subject> _subjects = database.GetCollection<Subject>("subjects");

    public async Task<ApiResponse<List<Subject>>> GetAllSubjectsAsync()
    {
        try
        {
            var subject = await _subjects.Find(_ => true).ToListAsync();
            return subject is null
                ? ApiResponse<List<Subject>>.ErrorResult("Subjects not found.", HttpStatusCode.NotFound)
                : ApiResponse<List<Subject>>.SuccessResult(subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching subject: {ex.Message}", ex.Message);
            return ApiResponse<List<Subject>>.ErrorResult("Error when fetching Subjects",
                HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<Subject>> AddSubjectAsync(CreateSubjectRequest request)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Name)
                || string.IsNullOrWhiteSpace(request.Code)
                || string.IsNullOrWhiteSpace(request.Credit)
               ) return ApiResponse<Subject>.ErrorResult("Validation error.", HttpStatusCode.BadRequest);

            var subject = new Subject
            {
                Name = request.Name,
                Code = request.Code,
                Credit = request.Credit,
            };
            await _subjects.InsertOneAsync(subject);
            return ApiResponse<Subject>.SuccessResult(subject);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding Subject: {ex.Message}", ex.Message);
            return ApiResponse<Subject>.ErrorResult("Error when adding Subject", HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<bool>> AddSubjectsAsync(CreateSubjectRequest[] requests)
    {
        try
        {
            var subjectsToInsert = (from s in requests
                    select new Subject
                    {
                        Name = s.Name,
                        Code = s.Code,
                        Credit = s.Credit
                    })
                .ToList();

            await _subjects.InsertManyAsync(subjectsToInsert);
            var count = subjectsToInsert.Count();
            return ApiResponse<bool>.SuccessResult(true, HttpStatusCode.Created,
                $"Successfully added {count} subjects.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding Subjects: {ex.Message}", ex.Message);
            return ApiResponse<bool>.ErrorResult("Error when adding Subjects", HttpStatusCode.InternalServerError);
        }
    }
}