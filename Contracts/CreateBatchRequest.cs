namespace EduInsights.Server.Contracts;

public record CreateBatchRequest
{
    public required string Name { get; set; }
    public required string InstituteId { get; set; }
}