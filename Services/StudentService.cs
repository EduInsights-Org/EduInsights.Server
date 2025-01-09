using EduInsights.Server.Entities;
using EduInsights.Server.Interfaces;
using MongoDB.Driver;

namespace EduInsights.Server.Services;

public class StudentService(IMongoDatabase database) : IStudentService
{
    private readonly IMongoCollection<Student> _studentsCollection = database.GetCollection<Student>("students");

    public async Task<List<Student>> AddStudentsAsync(List<Student> students)
    {
        try
        {
            await _studentsCollection.InsertManyAsync(students);
            return students;
        }
        catch (Exception e)
        {
            throw new Exception("An error occurred while creating the students.{e}", e);
        }
    }
}