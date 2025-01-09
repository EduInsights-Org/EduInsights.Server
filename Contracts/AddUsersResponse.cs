namespace EduInsights.Server.Contracts;

public class AddUsersResponse
{
    public bool Success { get; set; }
    public List<string> SuccessfullyAddedUsers { get; set; } = [];
    public List<string> InvalidUsers { get; set; } = [];
    public List<string> ExistingUsers { get; set; } = [];
    public required string Message { get; set; }
}