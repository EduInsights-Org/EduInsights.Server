namespace EduInsights.Server.Contracts;

public record RegisterUserRequest
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string Email { get; set; }
    public required string InstituteName { get; set; }
    public required string Password { get; set; }
    public required string Role { get; set; }
}