using EduInsights.Server.Entities;

namespace EduInsights.Server.Interfaces;

public interface IStudentService
{
    Task<List<Student>> AddStudentsAsync(List<Student> students);
}