namespace EduInsights.Server.Contracts;

public record CreateUserRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? IndexNumber { get; init; } 
    public required string Email { get; init; }
    public required string InstituteId { get; init; }
    public string? BatchId { get; init; } 
    public required string Password { get; init; }
    public required string Role { get; init; }
}