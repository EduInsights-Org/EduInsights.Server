namespace EduInsights.EndPoints;

public static class StudentsEndpoints
{
    private const string StudentsEndpointsName = "/api/v1/students";

    public static void MapStudentsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup(StudentsEndpointsName).WithTags("EduInsights endpoints");
    }
}