namespace EduInsights.Server.Contracts;

public class AddSubjectsResponse
{
    public required List<string> AddedSubjects { get; set; }
    public required List<string> InvalidSubjects { get; set; }
    public required List<string> ExistingSubjects { get; set; }
    public required string Message { get; set; }
}