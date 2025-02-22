using System.ComponentModel.DataAnnotations;

namespace EduInsights.Server.Contracts;

public record CreateUserRequest(
    [Required] [StringLength(40)] string FirstName,
    [Required] [StringLength(40)] string LastName,
    [Required] [StringLength(40)] string? IndexNumber, //can be null
    [Required] [StringLength(40)] string Email,
    [Required] [StringLength(40)] string InstituteId,
    [Required] [StringLength(40)] string? BatchId, //can be null
    [Required] [StringLength(40)] string Password,
    [Required] [StringLength(40)] string Role,
    [Required] bool IsEMailVerified
);