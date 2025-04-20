using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Enums;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class ResultService(IMongoDatabase database, ILogger<ResultService> logger, IStudentService studentService)
    : IResultService
{
    private readonly IMongoCollection<Result> _resultsCollection = database.GetCollection<Result>("results");

    public async Task<ApiResponse<Result>> AddResultAsync(CreateResultRequest result)
    {
        var studentFilter = Builders<Student>.Filter.Eq(s => s.IndexNumber, result.IndexNumber);
        var student = (await studentService.GetStudentByFilterAsync(studentFilter));
        if (!student.Success) return ApiResponse<Result>.ErrorResult(student.Message, student.StatusCode);

        var re = new Result()
        {
            Grade = result.Grade,
            StudentId = student.Data!.Id,
            SubjectId = result.SubjectId,
            SemesterId = result.SemesterId,
        };
        await _resultsCollection.InsertOneAsync(re);
        return ApiResponse<Result>.SuccessResult(re);
    }

    public async Task<ApiResponse<List<Result>>> GetAllResultsAsync()
    {
        var results = await _resultsCollection.Find(_ => true).ToListAsync();
        return results is null
            ? ApiResponse<List<Result>>.ErrorResult("Results not found",
                HttpStatusCode.NotFound)
            : ApiResponse<List<Result>>.SuccessResult(results);
    }
}