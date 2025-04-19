namespace EduInsights.Server.Contracts;

public class CreateSemesterRequest
{
    public required string Year { get; set; }
    public required string Sem { get; set; }
    public required string InstituteId { get; set; }
}