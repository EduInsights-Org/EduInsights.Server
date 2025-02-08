namespace EduInsights.Server.Contracts;

public class GetUserWithStudentResponse
{
    public required string Id { get; set; } = null!;
    public required string FirstName { get; set; } = null!;
    public required string LastName { get; set; } = null!;
    public required string Email { get; set; } = null!;
    public required string UserName { get; set; } = null!;
    public required string Role { get; set; } = null!;
    public required string? IndexNumber { get; set; }
};