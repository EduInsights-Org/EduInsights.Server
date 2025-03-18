namespace EduInsights.Server.Contracts;

public record UpdateUserResponse
{
    public long MatchedCount { get; set; }
    public long ModifiedCount { get; set; }
}