namespace EduInsights.Server.Contracts;

public record AddUsersResponse
{
    public bool Success { get; set; }
    public List<string> AddedUsers { get; set; } = [];
    public List<string> InvalidUsers { get; set; } = [];
    public List<string> ExistingUsers { get; set; } = [];
    public required string Message { get; set; }
}