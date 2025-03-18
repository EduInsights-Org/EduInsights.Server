namespace EduInsights.Server.Contracts;

public record GetUserWithStudentResponse
{
    public required string Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string Role { get; set; }
    public required string? IndexNumber { get; set; }
    public required bool IsEmailVerified { get; set; }
};