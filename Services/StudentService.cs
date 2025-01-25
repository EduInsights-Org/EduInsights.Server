using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class StudentService(IMongoDatabase database, ILogger<StudentService> logger) : IStudentService
{
    private readonly IMongoCollection<Student> _studentsCollection = database.GetCollection<Student>("students");

    public async Task<ApiResponse<string>> AddStudentAsync(Student studentRequest)
    {
        try
        {
            await _studentsCollection.InsertOneAsync(studentRequest);
            return ApiResponse<string>.SuccessResult("Successfully added student");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding students: {ex.Message}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when adding students", 500);
        }
    }

    public async Task<ApiResponse<string>> AddStudentsAsync(List<Student> studentsRequest)
    {
        try
        {
            await _studentsCollection.InsertManyAsync(studentsRequest);
            return ApiResponse<string>.SuccessResult("Successfully added students");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when adding students: {ex.Message}", ex.Message);
            return ApiResponse<string>.ErrorResult("Error when adding students", 500);
        }
    }


    public async Task<ApiResponse<List<Student>>> GetAllStudentAsync()
    {
        try
        {
            var students = await _studentsCollection.Find(_ => true).ToListAsync();
            return students is null
                ? ApiResponse<List<Student>>.ErrorResult("No students found", 404)
                : ApiResponse<List<Student>>.SuccessResult(students);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching Students: {ex.Message}", ex.Message);
            return ApiResponse<List<Student>>.ErrorResult("Error when fetching Students", 500);
        }
    }

    public async Task<ApiResponse<Student>> GetStudentByUserIdAsync(string userId)
    {
        try
        {
            var student = await _studentsCollection.Find(s => s.UserId == userId).FirstOrDefaultAsync();
            
            return student is null
                ? ApiResponse<Student>.ErrorResult("No students found", 404)
                : ApiResponse<Student>.SuccessResult(student);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error when fetching Students: {ex.Message}", ex.Message);
            return ApiResponse<Student>.ErrorResult("Error when fetching Students", 500);
        }
    }
}