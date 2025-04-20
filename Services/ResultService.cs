using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Enums;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class ResultService(
    IMongoDatabase database,
    ILogger<ResultService> logger,
    IStudentService studentService,
    ISemesterService semesterService,
    IBatchService batchService,
    ISubjectService subjectService)
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

    public async Task<ApiResponse<List<GetResultResponse>>> GetAllResultsAsync()
    {
        var resultList = new List<GetResultResponse>();

        var results = await _resultsCollection.Find(_ => true).ToListAsync();
        foreach (var result in results)
        {
            var studentFilter = Builders<Student>.Filter.Eq(s => s.Id, result.StudentId);
            var student = (await studentService.GetStudentByFilterAsync(studentFilter));
            var indexNumber = student.Data!.IndexNumber;

            var subjectFilter = Builders<Subject>.Filter.Eq(s => s.Id, result.SubjectId);
            var subject = await subjectService.GetSubjectByFilterAsync(subjectFilter);
            var subjectName = subject.Data!.Name;
            var subjectCode = subject.Data!.Code;

            var semesterFilter = Builders<Semester>.Filter.Eq(s => s.Id, result.SemesterId);
            var semester = await semesterService.GetSemesterByFilterAsync(semesterFilter);
            var semesterName = $"Year {semester.Data!.Year} Semester {semester.Data!.Sem}";

            var batchFilter = Builders<Batch>.Filter.Eq(s => s.Id, student.Data!.BatchId);
            var batch = await batchService.BatchByFilterAsync(batchFilter);

            var modifiedResult = new GetResultResponse
            {
                Batch = batch.Data!.Name,
                Grade = result.Grade,
                Semester = semesterName,
                SubjectName = subjectName,
                IndexNumber = indexNumber,
                SubjectCode = subjectCode,
            };
            resultList.Add(modifiedResult);
        }

        return resultList.Count == 0
            ? ApiResponse<List<GetResultResponse>>.ErrorResult("Results not found",
                HttpStatusCode.NotFound)
            : ApiResponse<List<GetResultResponse>>.SuccessResult(resultList);
    }
}