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
                InstituteId = request.InstituteId,
                Type = request.Type
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

    public async Task<ApiResponse<AddSubjectsResponse>> AddSubjectsAsync(CreateSubjectRequest[] requests)
    {
        var _subjectsToInsert = new List<Subject>();
        var _invalidSubjects = new List<string>();
        var _existingSubjects = new List<string>();

        try
        {
            foreach (var request in requests)
            {
                var subject = new Subject
                {
                    Name = request.Name,
                    Code = request.Code,
                    Credit = request.Credit,
                    InstituteId = request.InstituteId,
                    Type = request.Type,
                };

                if (string.IsNullOrWhiteSpace(request.Name) ||
                    string.IsNullOrWhiteSpace(request.Code) ||
                    string.IsNullOrWhiteSpace(request.Credit))
                {
                    var subjectName = subject.Name;
                    if (string.IsNullOrWhiteSpace(request.Name)) subjectName = "Unknown";
                    _invalidSubjects.Add(subjectName);
                    continue;
                }

                var existingSubjectFormCode = await _subjects.Find(s => s.Code == request.Code).FirstOrDefaultAsync();
                if (existingSubjectFormCode is not null)
                {
                    _existingSubjects.Add(subject.Name);
                    continue;
                }

                var existingSubjectFromName = await _subjects.Find(s => s.Name == request.Name).FirstOrDefaultAsync();
                if (existingSubjectFromName is not null)
                {
                    _existingSubjects.Add(subject.Name);
                    continue;
                }

                _subjectsToInsert.Add(subject);
            }

            if (_subjectsToInsert.Count > 0)
                await _subjects.InsertManyAsync(_subjectsToInsert);

            var addSubjectsResponse = new AddSubjectsResponse
            {
                AddedSubjects = _subjectsToInsert.Select(s => s.Name).ToList(),
                ExistingSubjects = _existingSubjects,
                InvalidSubjects = _invalidSubjects,
                Message =
                    $"Successfully added {_subjectsToInsert.Count} subjects. " +
                    $"{_invalidSubjects.Count} invalid subjects. " +
                    $"{_existingSubjects.Count} existing subjects.",
            };
            return ApiResponse<AddSubjectsResponse>.SuccessResult(addSubjectsResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding Subjects: {ex.Message}", ex.Message);
            return ApiResponse<AddSubjectsResponse>.ErrorResult("Error when adding Subjects",
                HttpStatusCode.InternalServerError);
        }
    }

    public async Task<ApiResponse<PaginatedResponse<List<Subject>>>> GetSubjectsAsync(
        string? instituteId, int page, int pageSize)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(instituteId))
                return ApiResponse<PaginatedResponse<List<Subject>>>.ErrorResult(
                    "Institute ID must be provided when getting subjects", HttpStatusCode.BadRequest);

            var filter = Builders<Subject>.Filter.Eq(s => s.InstituteId, instituteId);

            var subjectList = await _subjects.Find(filter).ToListAsync();
            if (subjectList is null)
                return ApiResponse<PaginatedResponse<List<Subject>>>.ErrorResult(
                    "No subjects found", HttpStatusCode.NotFound);

            //pagination logic
            var totalRecords = await _subjects.CountDocumentsAsync(filter);
            subjectList = subjectList.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var paginationResponse = new PaginatedResponse<List<Subject>>
            {
                Data = subjectList,
                TotalRecords = totalRecords,
                CurrentPage = page,
                PageSize = pageSize
            };

            return ApiResponse<PaginatedResponse<List<Subject>>>.SuccessResult(paginationResponse);
        }
        catch (FormatException ex)
        {
            logger.LogError(ex, "Invalid format for institute ID.");
            return ApiResponse<PaginatedResponse<List<Subject>>>.ErrorResult(
                "Invalid Institute ID format.", HttpStatusCode.BadRequest);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when getting sbjects");
            return ApiResponse<PaginatedResponse<List<Subject>>>.ErrorResult(
                "Error when getting subjects", HttpStatusCode.InternalServerError);
        }
    }
}