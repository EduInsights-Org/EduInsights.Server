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
    ISubjectService subjectService,
    IUserService userService)
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

    public async Task<ApiResponse<List<GetResultResponse>>> GetAllResultsAsync(string instituteId, string? batchId)
    {
        try
        {
            if (string.IsNullOrEmpty(instituteId))
                return ApiResponse<List<GetResultResponse>>.ErrorResult(
                    "Institute ID cannot be null or empty.", HttpStatusCode.BadRequest);

            var resultList = new List<GetResultResponse>();

            var results = await _resultsCollection.Find(r => r.InstituteId == instituteId).ToListAsync();

            foreach (var result in results)
            {
                var studentFilter = Builders<Student>.Filter.Eq(s => s.Id, result.StudentId);

                var student = (await studentService.GetStudentByFilterAsync(studentFilter));
                if (!student.Success)
                    return ApiResponse<List<GetResultResponse>>.ErrorResult(student.Message, student.StatusCode);

                if (batchId is not null && student.Data!.BatchId != batchId) continue;

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
        catch (FormatException ex)
        {
            logger.LogError(ex, "Invalid format for institute ID.");
            return ApiResponse<List<GetResultResponse>>.ErrorResult(
                "Invalid institute ID format.", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting Results.");
            return ApiResponse<List<GetResultResponse>>.ErrorResult(
                "Error when getting Results.", HttpStatusCode.InternalServerError);
        }
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

    public async Task<ApiResponse<List<StudentGpaResponse>>> CalculateAllStudentGPAsAsync(
        string? instituteId,
        string? batchId = null)
    {
        try
        {
            if (string.IsNullOrEmpty(instituteId))
            {
                return ApiResponse<List<StudentGpaResponse>>.ErrorResult(
                    "Institute ID cannot be null or empty.", HttpStatusCode.BadRequest);
            }

            // Get all students for the institute
            var studentFilter = Builders<Student>.Filter.Eq(s => s.InstituteId, instituteId);

            if (!string.IsNullOrEmpty(batchId))
            {
                studentFilter &= Builders<Student>.Filter.Eq(s => s.BatchId, batchId);
            }

            var students = await studentService.GetStudentsByFilterAsync(studentFilter);

            if (!students.Success || students.Data == null || !students.Data.Any())
            {
                return ApiResponse<List<StudentGpaResponse>>.ErrorResult(
                    "No students found matching the criteria.", HttpStatusCode.NotFound);
            }

            var gpaResults = new List<StudentGpaResponse>();

            foreach (var student in students.Data)
            {
                // Get user details for first/last name
                // var userFilter = Builders<User>.Filter.Eq(u => u.Id, student.UserId);
                var user = await userService.GetUserByIdAsync(student.UserId);
                if (!user.Success)
                    return ApiResponse<List<StudentGpaResponse>>.ErrorResult(
                        user.Message, user.StatusCode);

                // Get all results for the student
                var resultFilter = Builders<Result>.Filter.Eq(r => r.StudentId, student.Id);
                var results = await _resultsCollection.Find(resultFilter).ToListAsync();

                if (results.Count == 0) continue;

                double totalGradePoints = 0;
                var totalCredits = 0;
                var subjectCount = results.Count;

                foreach (var result in results)
                {
                    // Get subject to get credit value
                    var subjectFilter = Builders<Subject>.Filter.Eq(s => s.Id, result.SubjectId);
                    var subject = await subjectService.GetSubjectByFilterAsync(subjectFilter);

                    if (subject.Success && subject.Data != null)
                    {
                        if (int.TryParse(subject.Data.Credit, out int credit))
                        {
                            totalGradePoints += ConvertGradeToPoints(result.Grade) * credit;
                            totalCredits += credit;
                        }
                    }
                }

                // Get batch name
                var batchFilter = Builders<Batch>.Filter.Eq(b => b.Id, student.BatchId);
                var batch = await batchService.BatchByFilterAsync(batchFilter);
                var batchName = batch.Success && batch.Data != null ? batch.Data.Name : "Unknown";

                double gpa = totalCredits > 0 ? totalGradePoints / totalCredits : 0;

                gpaResults.Add(new StudentGpaResponse
                {
                    IndexNumber = student.IndexNumber,
                    Batch = batchName,
                    Gpa = Math.Round(gpa, 2),
                    SubjectCount = subjectCount,
                    FirstName = user.Data!.FirstName,
                    LastName = user.Data!.LastName
                });
            }

            return gpaResults.Count == 0
                ? ApiResponse<List<StudentGpaResponse>>.ErrorResult(
                    "No GPA results found",
                    HttpStatusCode.NotFound)
                : ApiResponse<List<StudentGpaResponse>>.SuccessResult(gpaResults);
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Invalid format for institute ID or Batch ID.");
            return ApiResponse<List<StudentGpaResponse>>.ErrorResult(
                "Invalid ID format.", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when calculating student GPAs");
            return ApiResponse<List<StudentGpaResponse>>.ErrorResult(
                "Error when calculating student GPAs",
                HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<List<BatchGpaResponse>>> GetBatchAverageGPAsAsync(string? instituteId)
    {
        try
        {
            if (string.IsNullOrEmpty(instituteId))
            {
                return ApiResponse<List<BatchGpaResponse>>.ErrorResult(
                    "Institute ID cannot be null or empty.",
                    HttpStatusCode.BadRequest);
            }

            // Get all batches for the institute
            var batches = await batchService.GetBatchesByInstituteIdAsync(instituteId);
            if (!batches.Success)
                return ApiResponse<List<BatchGpaResponse>>.ErrorResult(
                    batches.Message, HttpStatusCode.InternalServerError);

            if (batches.Data!.Count == 0)
            {
                return ApiResponse<List<BatchGpaResponse>>.ErrorResult(
                    "No batches found for this institute.",
                    HttpStatusCode.NotFound);
            }

            var batchGpas = new List<BatchGpaResponse>();

            foreach (var batch in batches.Data!)
            {
                // Get all students in this batch
                var studentFilter = Builders<Student>.Filter.Eq(s => s.BatchId, batch.Id);
                var students = await studentService.GetStudentsByFilterAsync(studentFilter);
                if (!students.Success)
                    return ApiResponse<List<BatchGpaResponse>>.ErrorResult(
                        students.Message, HttpStatusCode.InternalServerError);

                if (students.Data!.Count == 0) continue;

                double batchTotalGpa = 0;
                int studentsWithGpa = 0;

                foreach (var student in students.Data!)
                {
                    // Get all results for the student
                    var resultFilter = Builders<Result>.Filter.Eq(r => r.StudentId, student.Id);
                    var results = await _resultsCollection.Find(resultFilter).ToListAsync();

                    if (results.Count == 0) continue;

                    double totalGradePoints = 0;
                    int totalCredits = 0;

                    foreach (var result in results)
                    {
                        // Get subject to get credit value
                        var subjectFilter = Builders<Subject>.Filter.Eq(s => s.Id, result.SubjectId);
                        var subject = await subjectService.GetSubjectByFilterAsync(subjectFilter);

                        if (subject.Success && subject.Data != null)
                        {
                            if (int.TryParse(subject.Data.Credit, out int credit))
                            {
                                totalGradePoints += ConvertGradeToPoints(result.Grade) * credit;
                                totalCredits += credit;
                            }
                        }
                    }

                    if (totalCredits > 0)
                    {
                        double studentGpa = totalGradePoints / totalCredits;
                        batchTotalGpa += studentGpa;
                        studentsWithGpa++;
                    }
                }

                if (studentsWithGpa > 0)
                {
                    batchGpas.Add(new BatchGpaResponse
                    {
                        BatchId = batch.Id,
                        BatchName = batch.Name,
                        AverageGpa = Math.Round(batchTotalGpa / studentsWithGpa, 2),
                        StudentCount = studentsWithGpa,
                        TotalStudentsInBatch = students.Data.Count
                    });
                }
            }

            return batchGpas.Count == 0
                ? ApiResponse<List<BatchGpaResponse>>.ErrorResult(
                    "No GPA data found for any batches.",
                    HttpStatusCode.NotFound)
                : ApiResponse<List<BatchGpaResponse>>.SuccessResult(batchGpas);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error calculating batch average GPAs");
            return ApiResponse<List<BatchGpaResponse>>.ErrorResult(
                "Error calculating batch average GPAs",
                HttpStatusCode.InternalServerError);
        }
    }

    private double ConvertGradeToPoints(string grade)
    {
        return grade switch
        {
            "A+" => 4.0,
            "A" => 4.0,
            "A-" => 3.7,
            "B+" => 3.3,
            "B" => 3.0,
            "B-" => 2.7,
            "C+" => 2.3,
            "C" => 2.0,
            "C-" => 1.7,
            "D+" => 1.3,
            "D" => 1.0,
            "D-" => 0.7,
            "E" => 0.0,
            _ => 0.0
        };
    }
}