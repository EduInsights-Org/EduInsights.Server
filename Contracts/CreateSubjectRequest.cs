namespace EduInsights.Server.Contracts;

public class CreateSubjectRequest
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required string Credit { get; set; }
    public required string InstituteId { get; set; }
}