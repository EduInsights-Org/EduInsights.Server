using EduInsights.Server.Interfaces;

namespace EduInsights.Server.EndPoints;

public static class StudentsEndpoints
{
    private const string StudentsEndpointsName = "/api/v1/students";

    public static void MapStudentsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(StudentsEndpointsName).WithTags("EduInsights endpoints");

        group.MapGet("/", async (IStudentService studentService) =>
        {
            var result = await studentService.GetAllStudentAsync();
            return Results.Json(result, statusCode: result.StatusCode);
        });
    }
}