namespace EduInsights.Server.Contracts;

public record UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? InstituteId { get; set; }
    public string? Password { get; set; }
    public string? Role { get; set; }
    public bool? IsEMailVerified { get; set; }
}