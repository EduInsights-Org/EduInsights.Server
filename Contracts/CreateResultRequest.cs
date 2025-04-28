namespace EduInsights.Server.Contracts;

public class CreateResultRequest
{
    public required string IndexNumber { get; set; }
    public required string Grade { get; set; }
    public required string SemesterId { get; set; }
    public required string SubjectId { get; set; }
    public required string InstituteId { get; set; }
}