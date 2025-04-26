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
            InstituteId = result.InstituteId,
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

    public async Task<ApiResponse<GetGradeDistribution>> GetGradeDistribution(string? instituteId)
    {
        try
        {
            if (string.IsNullOrEmpty(instituteId))
                return ApiResponse<GetGradeDistribution>.ErrorResult(
                    "Institute ID cannot be null or empty.", HttpStatusCode.BadRequest);

            var results = await _resultsCollection
                .Find(r => r.InstituteId == instituteId)
                .Project(result => new { result.Grade })
                .ToListAsync();

            var gradeDistribution = new GetGradeDistribution();

            foreach (var result in results)
            {
                switch (result.Grade)
                {
                    case "A+":
                        gradeDistribution.APlus++;
                        break;
                    case "A":
                        gradeDistribution.A++;
                        break;
                    case "A-":
                        gradeDistribution.AMinus++;
                        break;
                    case "B+":
                        gradeDistribution.BPlus++;
                        break;
                    case "B":
                        gradeDistribution.B++;
                        break;
                    case "B-":
                        gradeDistribution.BMinus++;
                        break;
                    case "C+":
                        gradeDistribution.CPlus++;
                        break;
                    case "C":
                        gradeDistribution.C++;
                        break;
                    case "C-":
                        gradeDistribution.CMinus++;
                        break;
                    case "D+":
                        gradeDistribution.DPlus++;
                        break;
                    case "D":
                        gradeDistribution.D++;
                        break;
                    case "D-":
                        gradeDistribution.DMinus++;
                        break;
                    case "E":
                        gradeDistribution.E++;
                        break;
                }
            }

            return ApiResponse<GetGradeDistribution>.SuccessResult(gradeDistribution);
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Invalid format for institute ID.");
            return ApiResponse<GetGradeDistribution>.ErrorResult(
                "Invalid institute ID format.", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting grades.");
            return ApiResponse<GetGradeDistribution>.ErrorResult(
                "Error when getting grdes.", HttpStatusCode.InternalServerError);
        }
    }
}