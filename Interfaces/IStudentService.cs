using EduInsights.Server.Contracts;
using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IStudentService
{
    Task<ApiResponse<string>> AddStudentAsync(Student student);
    Task<ApiResponse<string>> AddStudentsAsync(List<Student> students);
    Task<ApiResponse<List<Student>>> GetAllStudentAsync();
    Task<ApiResponse<Student>> GetStudentByUserIdAsync(string userId);
}