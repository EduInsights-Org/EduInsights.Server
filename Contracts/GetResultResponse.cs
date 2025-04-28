namespace EduInsights.Server.Contracts;

public class GetResultResponse
{
    public required string Grade { get; set; }
    public required string Semester { get; set; }
    public required string SubjectName { get; set; }
    
    public required string SubjectCode { get; set; }
    public required string IndexNumber { get; set; }
    public required string Batch { get; set; }
}